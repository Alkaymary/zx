#!/usr/bin/env bash

set -Eeuo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "${SCRIPT_DIR}/.." && pwd)"
ENV_FILE="${PROJECT_ROOT}/.env"
ENV_EXAMPLE_FILE="${PROJECT_ROOT}/.env.example"

log() {
  printf '[build-ubuntu] %s\n' "$1"
}

fail() {
  printf '[build-ubuntu] ERROR: %s\n' "$1" >&2
  exit 1
}

require_ubuntu() {
  if [[ ! -f /etc/os-release ]]; then
    fail "Cannot detect operating system."
  fi

  # shellcheck disable=SC1091
  source /etc/os-release
  if [[ "${ID:-}" != "ubuntu" ]]; then
    fail "This script is intended for Ubuntu. Detected: ${PRETTY_NAME:-unknown}."
  fi
}

command_exists() {
  command -v "$1" >/dev/null 2>&1
}

ensure_sudo() {
  if ! command_exists sudo; then
    fail "sudo is required."
  fi
}

install_docker_if_missing() {
  if command_exists docker && docker compose version >/dev/null 2>&1; then
    log "Docker Engine and Docker Compose plugin are already installed."
    return
  fi

  log "Installing Docker Engine and Docker Compose plugin..."
  sudo apt-get update
  sudo apt-get install -y ca-certificates curl
  sudo apt-get remove -y docker.io docker-compose docker-compose-v2 docker-doc podman-docker containerd runc >/dev/null 2>&1 || true
  sudo install -m 0755 -d /etc/apt/keyrings

  if [[ ! -f /etc/apt/keyrings/docker.asc ]]; then
    sudo curl -fsSL https://download.docker.com/linux/ubuntu/gpg -o /etc/apt/keyrings/docker.asc
    sudo chmod a+r /etc/apt/keyrings/docker.asc
  fi

  local codename
  codename="$(. /etc/os-release && echo "${UBUNTU_CODENAME:-$VERSION_CODENAME}")"

  sudo tee /etc/apt/sources.list.d/docker.sources >/dev/null <<EOF
Types: deb
URIs: https://download.docker.com/linux/ubuntu
Suites: ${codename}
Components: stable
Signed-By: /etc/apt/keyrings/docker.asc
EOF

  sudo apt-get update
  sudo apt-get install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin
  sudo systemctl enable --now docker
}

random_value() {
  if command_exists openssl; then
    openssl rand -hex "$1"
    return
  fi

  date +%s%N | sha256sum | cut -c1-"$(( $1 * 2 ))"
}

create_env_file_if_missing() {
  if [[ -f "${ENV_FILE}" ]]; then
    log ".env file already exists."
    return
  fi

  if [[ -f "${ENV_EXAMPLE_FILE}" ]]; then
    cp "${ENV_EXAMPLE_FILE}" "${ENV_FILE}"
  else
    touch "${ENV_FILE}"
  fi

  local postgres_password jwt_secret admin_password
  postgres_password="$(random_value 16)"
  jwt_secret="$(random_value 32)"
  admin_password="$(random_value 12)"

  cat > "${ENV_FILE}" <<EOF
COMPOSE_PROJECT_NAME=myapi

MYAPI_HTTP_PORT=8080
POSTGRES_PORT=5432

POSTGRES_DB=myapi
POSTGRES_USER=myapi
POSTGRES_PASSWORD=${postgres_password}

ASPNETCORE_ENVIRONMENT=Production

MYAPI_JWT_ISSUER=MyApi
MYAPI_JWT_AUDIENCE=MyApiClient
MYAPI_JWT_SECRET=${jwt_secret}

MYAPI_DATABASE_APPLY_MIGRATIONS_ON_STARTUP=true

MYAPI_BOOTSTRAP_ADMIN_ENABLED=true
MYAPI_BOOTSTRAP_ADMIN_FULLNAME=SuperAdmin
MYAPI_BOOTSTRAP_ADMIN_USERNAME=superadmin
MYAPI_BOOTSTRAP_ADMIN_EMAIL=superadmin@example.local
MYAPI_BOOTSTRAP_ADMIN_PHONE=07700000000
MYAPI_BOOTSTRAP_ADMIN_ROLE=super_admin
MYAPI_BOOTSTRAP_ADMIN_PASSWORD=${admin_password}

SERILOG_SEQ_URL=
EOF

  chmod 600 "${ENV_FILE}"
  log "Created .env with generated production-friendly secrets."
}

ensure_directories() {
  mkdir -p "${PROJECT_ROOT}/logs"
  mkdir -p "${PROJECT_ROOT}/docker/backups"
}

compose() {
  sudo docker compose \
    --project-directory "${PROJECT_ROOT}" \
    --env-file "${ENV_FILE}" \
    -f "${PROJECT_ROOT}/docker-compose.yml" \
    "$@"
}

start_stack() {
  log "Building and starting containers..."
  compose up -d --build --remove-orphans
}

wait_for_health() {
  log "Waiting for API readiness..."
  local attempt
  for attempt in $(seq 1 30); do
    if compose exec -T api curl --fail --silent http://localhost:8080/health/ready >/dev/null; then
      log "Application is healthy."
      return
    fi

    sleep 5
  done

  compose ps || true
  compose logs api --tail=120 || true
  fail "Application did not become healthy in time."
}

print_summary() {
  local http_port admin_user admin_password
  http_port="$(grep -E '^MYAPI_HTTP_PORT=' "${ENV_FILE}" | cut -d'=' -f2-)"
  admin_user="$(grep -E '^MYAPI_BOOTSTRAP_ADMIN_USERNAME=' "${ENV_FILE}" | cut -d'=' -f2-)"
  admin_password="$(grep -E '^MYAPI_BOOTSTRAP_ADMIN_PASSWORD=' "${ENV_FILE}" | cut -d'=' -f2-)"

  log "Done."
  printf 'URL: http://<server-ip>:%s\n' "${http_port}"
  printf 'Admin username: %s\n' "${admin_user}"
  printf 'Admin password: %s\n' "${admin_password}"
  printf '.env path: %s\n' "${ENV_FILE}"
}

main() {
  require_ubuntu
  ensure_sudo
  install_docker_if_missing
  create_env_file_if_missing
  ensure_directories
  start_stack
  wait_for_health
  print_summary
}

main "$@"
