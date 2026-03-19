--
-- PostgreSQL database dump
--

\restrict Hyb1su7qoyjjsSVIAtUrCH1avGKtbqWJ92XOog1uDVbwcDE5Gxi2kY5saHdjS1g

-- Dumped from database version 16.13
-- Dumped by pg_dump version 16.13

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

ALTER TABLE IF EXISTS ONLY public.transaction_settlements DROP CONSTRAINT IF EXISTS "FK_transaction_settlements_financial_transactions_financial_tr~";
ALTER TABLE IF EXISTS ONLY public.transaction_settlements DROP CONSTRAINT IF EXISTS "FK_transaction_settlements_admin_users_created_by_admin_user_id";
ALTER TABLE IF EXISTS ONLY public.refresh_tokens DROP CONSTRAINT IF EXISTS "FK_refresh_tokens_library_accounts_library_account_id";
ALTER TABLE IF EXISTS ONLY public.refresh_tokens DROP CONSTRAINT IF EXISTS "FK_refresh_tokens_admin_users_admin_user_id";
ALTER TABLE IF EXISTS ONLY public.qr_codes DROP CONSTRAINT IF EXISTS "FK_qr_codes_pos_devices_pos_device_id";
ALTER TABLE IF EXISTS ONLY public.qr_codes DROP CONSTRAINT IF EXISTS "FK_qr_codes_packages_package_id";
ALTER TABLE IF EXISTS ONLY public.qr_codes DROP CONSTRAINT IF EXISTS "FK_qr_codes_library_accounts_created_by_library_account_id";
ALTER TABLE IF EXISTS ONLY public.qr_codes DROP CONSTRAINT IF EXISTS "FK_qr_codes_libraries_library_id";
ALTER TABLE IF EXISTS ONLY public.qr_codes DROP CONSTRAINT IF EXISTS "FK_qr_codes_financial_transactions_financial_transaction_id";
ALTER TABLE IF EXISTS ONLY public.pos_devices DROP CONSTRAINT IF EXISTS "FK_pos_devices_library_accounts_activated_by_account_id";
ALTER TABLE IF EXISTS ONLY public.pos_devices DROP CONSTRAINT IF EXISTS "FK_pos_devices_libraries_library_id";
ALTER TABLE IF EXISTS ONLY public.packages DROP CONSTRAINT IF EXISTS "FK_packages_admin_users_added_by_admin_user_id";
ALTER TABLE IF EXISTS ONLY public.library_accounts DROP CONSTRAINT IF EXISTS "FK_library_accounts_roles_role_id";
ALTER TABLE IF EXISTS ONLY public.library_accounts DROP CONSTRAINT IF EXISTS "FK_library_accounts_libraries_library_id";
ALTER TABLE IF EXISTS ONLY public.financial_transactions DROP CONSTRAINT IF EXISTS "FK_financial_transactions_library_accounts_created_by_library_~";
ALTER TABLE IF EXISTS ONLY public.financial_transactions DROP CONSTRAINT IF EXISTS "FK_financial_transactions_libraries_library_id";
ALTER TABLE IF EXISTS ONLY public.financial_transactions DROP CONSTRAINT IF EXISTS "FK_financial_transactions_admin_users_created_by_admin_user_id";
ALTER TABLE IF EXISTS ONLY public.admin_users DROP CONSTRAINT IF EXISTS "FK_admin_users_roles_role_id";
DROP INDEX IF EXISTS public."IX_transaction_settlements_financial_transaction_id";
DROP INDEX IF EXISTS public."IX_transaction_settlements_created_by_admin_user_id";
DROP INDEX IF EXISTS public."IX_roles_code";
DROP INDEX IF EXISTS public."IX_refresh_tokens_token";
DROP INDEX IF EXISTS public."IX_refresh_tokens_library_account_id";
DROP INDEX IF EXISTS public."IX_refresh_tokens_admin_user_id";
DROP INDEX IF EXISTS public."IX_qr_codes_qr_reference";
DROP INDEX IF EXISTS public."IX_qr_codes_pos_device_id";
DROP INDEX IF EXISTS public."IX_qr_codes_package_id";
DROP INDEX IF EXISTS public."IX_qr_codes_library_id";
DROP INDEX IF EXISTS public."IX_qr_codes_financial_transaction_id";
DROP INDEX IF EXISTS public."IX_qr_codes_created_by_library_account_id";
DROP INDEX IF EXISTS public."IX_pos_devices_pos_code";
DROP INDEX IF EXISTS public."IX_pos_devices_library_id";
DROP INDEX IF EXISTS public."IX_pos_devices_activated_by_account_id";
DROP INDEX IF EXISTS public."IX_packages_package_code";
DROP INDEX IF EXISTS public."IX_packages_name";
DROP INDEX IF EXISTS public."IX_packages_added_by_admin_user_id";
DROP INDEX IF EXISTS public."IX_library_accounts_username";
DROP INDEX IF EXISTS public."IX_library_accounts_role_id";
DROP INDEX IF EXISTS public."IX_library_accounts_library_id";
DROP INDEX IF EXISTS public."IX_libraries_library_code";
DROP INDEX IF EXISTS public."IX_financial_transactions_library_id";
DROP INDEX IF EXISTS public."IX_financial_transactions_created_by_library_account_id";
DROP INDEX IF EXISTS public."IX_financial_transactions_created_by_admin_user_id";
DROP INDEX IF EXISTS public."IX_audit_logs_trace_identifier_trgm";
DROP INDEX IF EXISTS public."IX_audit_logs_trace_identifier";
DROP INDEX IF EXISTS public."IX_audit_logs_status";
DROP INDEX IF EXISTS public."IX_audit_logs_security_level";
DROP INDEX IF EXISTS public."IX_audit_logs_operation_date_id";
DROP INDEX IF EXISTS public."IX_audit_logs_operation_date";
DROP INDEX IF EXISTS public."IX_audit_logs_ip_address_trgm";
DROP INDEX IF EXISTS public."IX_audit_logs_ip_address";
DROP INDEX IF EXISTS public."IX_audit_logs_endpoint_trgm";
DROP INDEX IF EXISTS public."IX_audit_logs_endpoint";
DROP INDEX IF EXISTS public."IX_audit_logs_action_name_trgm";
DROP INDEX IF EXISTS public."IX_audit_logs_action_name";
DROP INDEX IF EXISTS public."IX_audit_logs_account_username_trgm";
DROP INDEX IF EXISTS public."IX_audit_logs_account_username";
DROP INDEX IF EXISTS public."IX_admin_users_username";
DROP INDEX IF EXISTS public."IX_admin_users_role_id";
DROP INDEX IF EXISTS public."IX_admin_users_email";
ALTER TABLE IF EXISTS ONLY public.transaction_settlements DROP CONSTRAINT IF EXISTS "PK_transaction_settlements";
ALTER TABLE IF EXISTS ONLY public.roles DROP CONSTRAINT IF EXISTS "PK_roles";
ALTER TABLE IF EXISTS ONLY public.refresh_tokens DROP CONSTRAINT IF EXISTS "PK_refresh_tokens";
ALTER TABLE IF EXISTS ONLY public.qr_codes DROP CONSTRAINT IF EXISTS "PK_qr_codes";
ALTER TABLE IF EXISTS ONLY public.pos_devices DROP CONSTRAINT IF EXISTS "PK_pos_devices";
ALTER TABLE IF EXISTS ONLY public.packages DROP CONSTRAINT IF EXISTS "PK_packages";
ALTER TABLE IF EXISTS ONLY public.library_accounts DROP CONSTRAINT IF EXISTS "PK_library_accounts";
ALTER TABLE IF EXISTS ONLY public.libraries DROP CONSTRAINT IF EXISTS "PK_libraries";
ALTER TABLE IF EXISTS ONLY public.financial_transactions DROP CONSTRAINT IF EXISTS "PK_financial_transactions";
ALTER TABLE IF EXISTS ONLY public.audit_logs DROP CONSTRAINT IF EXISTS "PK_audit_logs";
ALTER TABLE IF EXISTS ONLY public.admin_users DROP CONSTRAINT IF EXISTS "PK_admin_users";
ALTER TABLE IF EXISTS ONLY public."__EFMigrationsHistory" DROP CONSTRAINT IF EXISTS "PK___EFMigrationsHistory";
DROP TABLE IF EXISTS public.transaction_settlements;
DROP TABLE IF EXISTS public.roles;
DROP TABLE IF EXISTS public.refresh_tokens;
DROP TABLE IF EXISTS public.qr_codes;
DROP TABLE IF EXISTS public.pos_devices;
DROP TABLE IF EXISTS public.packages;
DROP TABLE IF EXISTS public.library_accounts;
DROP TABLE IF EXISTS public.libraries;
DROP TABLE IF EXISTS public.financial_transactions;
DROP TABLE IF EXISTS public.audit_logs;
DROP TABLE IF EXISTS public.admin_users;
DROP TABLE IF EXISTS public."__EFMigrationsHistory";
DROP EXTENSION IF EXISTS pg_trgm;
--
-- Name: pg_trgm; Type: EXTENSION; Schema: -; Owner: -
--

CREATE EXTENSION IF NOT EXISTS pg_trgm WITH SCHEMA public;


--
-- Name: EXTENSION pg_trgm; Type: COMMENT; Schema: -; Owner: -
--

COMMENT ON EXTENSION pg_trgm IS 'text similarity measurement and index searching based on trigrams';


SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- Name: __EFMigrationsHistory; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL
);


--
-- Name: admin_users; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.admin_users (
    id integer NOT NULL,
    full_name character varying(200) NOT NULL,
    username character varying(200) NOT NULL,
    email character varying(255),
    phone_number character varying(50),
    password_hash character varying(500) NOT NULL,
    role_id integer NOT NULL,
    status text NOT NULL,
    last_login_at timestamp with time zone,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    updated_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL
);


--
-- Name: admin_users_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public.admin_users ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public.admin_users_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: audit_logs; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.audit_logs (
    id integer NOT NULL,
    trace_identifier text NOT NULL,
    operation_date timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    account_type text NOT NULL,
    account_id integer,
    account_username text,
    role_code text,
    endpoint text NOT NULL,
    query_string text,
    action_name text NOT NULL,
    http_method text NOT NULL,
    request_payload text,
    response_payload text,
    status_code integer NOT NULL,
    status text NOT NULL,
    security_level text NOT NULL,
    ip_address text,
    duration_ms bigint NOT NULL
);


--
-- Name: audit_logs_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public.audit_logs ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public.audit_logs_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: financial_transactions; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.financial_transactions (
    id integer NOT NULL,
    library_id integer NOT NULL,
    transaction_type text NOT NULL,
    amount numeric NOT NULL,
    paid_amount numeric NOT NULL,
    remaining_amount numeric NOT NULL,
    description character varying(500),
    transaction_date timestamp with time zone NOT NULL,
    due_date timestamp with time zone,
    status text NOT NULL,
    created_by_admin_user_id integer,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    updated_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    created_by_library_account_id integer
);


--
-- Name: financial_transactions_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public.financial_transactions ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public.financial_transactions_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: libraries; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.libraries (
    id integer NOT NULL,
    library_code character varying(50) DEFAULT ''::character varying NOT NULL,
    library_name character varying(200) DEFAULT ''::character varying NOT NULL,
    owner_name character varying(200),
    owner_phone character varying(50),
    owner_phone_2 character varying(50),
    address character varying(300),
    province character varying(100),
    city character varying(100),
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    credit_limit numeric DEFAULT 0.0 NOT NULL,
    current_balance numeric DEFAULT 0.0 NOT NULL,
    latitude numeric,
    longitude numeric,
    notes text,
    status text DEFAULT 'active'::text NOT NULL,
    updated_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL
);


--
-- Name: libraries_Id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public.libraries ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."libraries_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: library_accounts; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.library_accounts (
    id integer NOT NULL,
    full_name character varying(200) DEFAULT ''::character varying NOT NULL,
    phone_number character varying(50),
    username character varying(200) DEFAULT ''::character varying NOT NULL,
    password_hash character varying(500) DEFAULT ''::character varying NOT NULL,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    status text NOT NULL,
    library_id integer DEFAULT 0 NOT NULL,
    last_login_at timestamp with time zone,
    role_id integer DEFAULT 5 NOT NULL,
    updated_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL
);


--
-- Name: library_accounts_Id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public.library_accounts ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."library_accounts_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: packages; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.packages (
    id integer NOT NULL,
    name character varying(200) NOT NULL,
    price_iqd numeric NOT NULL,
    status text NOT NULL,
    added_by_admin_user_id integer NOT NULL,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    updated_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    package_code character varying(100) DEFAULT ''::character varying NOT NULL
);


--
-- Name: packages_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public.packages ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public.packages_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: pos_devices; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.pos_devices (
    id integer NOT NULL,
    pos_code character varying(50) DEFAULT ''::character varying NOT NULL,
    serial_number character varying(150),
    device_model character varying(100),
    device_vendor character varying(100),
    status text NOT NULL,
    is_activated boolean NOT NULL,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    activated_at timestamp with time zone,
    last_authenticated_at timestamp with time zone,
    library_id integer,
    activated_by_account_id integer,
    activation_token character varying(200),
    updated_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL
);


--
-- Name: pos_devices_Id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public.pos_devices ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."pos_devices_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: qr_codes; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.qr_codes (
    id integer NOT NULL,
    library_id integer NOT NULL,
    pos_device_id integer NOT NULL,
    created_by_library_account_id integer NOT NULL,
    qr_reference character varying(100) NOT NULL,
    student_name character varying(200) NOT NULL,
    student_phone_number character varying(50) NOT NULL,
    qr_payload character varying(2000) NOT NULL,
    status text NOT NULL,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    updated_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    package_id integer DEFAULT 0 NOT NULL,
    financial_transaction_id integer DEFAULT 0 NOT NULL
);


--
-- Name: qr_codes_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public.qr_codes ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public.qr_codes_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: refresh_tokens; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.refresh_tokens (
    id integer NOT NULL,
    token character varying(200) NOT NULL,
    admin_user_id integer,
    library_account_id integer,
    expires_at timestamp with time zone NOT NULL,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    revoked_at timestamp with time zone,
    user_type character varying(100) NOT NULL
);


--
-- Name: refresh_tokens_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public.refresh_tokens ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public.refresh_tokens_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: roles; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.roles (
    id integer NOT NULL,
    name character varying(200) NOT NULL,
    code character varying(100) NOT NULL,
    guard_name text NOT NULL,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL
);


--
-- Name: roles_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public.roles ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public.roles_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: transaction_settlements; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.transaction_settlements (
    id integer NOT NULL,
    financial_transaction_id integer NOT NULL,
    amount numeric NOT NULL,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    created_by_admin_user_id integer,
    notes text,
    quantity numeric,
    settlement_date timestamp with time zone DEFAULT '-infinity'::timestamp with time zone NOT NULL,
    settlement_mode text DEFAULT ''::text NOT NULL,
    unit_amount numeric
);


--
-- Name: transaction_settlements_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public.transaction_settlements ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public.transaction_settlements_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Data for Name: __EFMigrationsHistory; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."__EFMigrationsHistory" ("MigrationId", "ProductVersion") FROM stdin;
20260313032609_InitialCreate	9.0.10
20260313035316_AddPosDevices	9.0.10
20260313040809_AlignCoreSchema	9.0.10
20260313041519_AddAdminUsers	9.0.10
20260313053106_AddRefreshTokens	9.0.10
20260313175612_AddPackagesModule	9.0.10
20260313180035_AddPackageCodeToPackages	9.0.10
20260313183306_AddFinancialModule	9.0.10
20260313184620_AddQrCodesModule	9.0.10
20260313184855_AddPackageRelationToQrCodes	9.0.10
20260313190138_LinkQrExportsToFinancialTransactions	9.0.10
20260314034212_RefactorFinancialSettlements	9.0.10
20260314035219_NormalizeFinancialData	9.0.10
20260315005257_AddAuditLogsModule	9.0.10
20260316010459_HardenAuditLogIndexesAndPgTrgm	9.0.10
\.


--
-- Data for Name: admin_users; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.admin_users (id, full_name, username, email, phone_number, password_hash, role_id, status, last_login_at, created_at, updated_at) FROM stdin;
1	Super Admin	superadmin	superadmin@example.local	07700000000	admin123456	1	active	\N	2026-03-19 06:04:52.098568+00	2026-03-19 06:05:10.559354+00
\.


--
-- Data for Name: audit_logs; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.audit_logs (id, trace_identifier, operation_date, account_type, account_id, account_username, role_code, endpoint, query_string, action_name, http_method, request_payload, response_payload, status_code, status, security_level, ip_address, duration_ms) FROM stdin;
\.


--
-- Data for Name: financial_transactions; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.financial_transactions (id, library_id, transaction_type, amount, paid_amount, remaining_amount, description, transaction_date, due_date, status, created_by_admin_user_id, created_at, updated_at, created_by_library_account_id) FROM stdin;
\.


--
-- Data for Name: libraries; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.libraries (id, library_code, library_name, owner_name, owner_phone, owner_phone_2, address, province, city, created_at, credit_limit, current_balance, latitude, longitude, notes, status, updated_at) FROM stdin;
\.


--
-- Data for Name: library_accounts; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.library_accounts (id, full_name, phone_number, username, password_hash, created_at, status, library_id, last_login_at, role_id, updated_at) FROM stdin;
\.


--
-- Data for Name: packages; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.packages (id, name, price_iqd, status, added_by_admin_user_id, created_at, updated_at, package_code) FROM stdin;
\.


--
-- Data for Name: pos_devices; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.pos_devices (id, pos_code, serial_number, device_model, device_vendor, status, is_activated, created_at, activated_at, last_authenticated_at, library_id, activated_by_account_id, activation_token, updated_at) FROM stdin;
\.


--
-- Data for Name: qr_codes; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.qr_codes (id, library_id, pos_device_id, created_by_library_account_id, qr_reference, student_name, student_phone_number, qr_payload, status, created_at, updated_at, package_id, financial_transaction_id) FROM stdin;
\.


--
-- Data for Name: refresh_tokens; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.refresh_tokens (id, token, admin_user_id, library_account_id, expires_at, created_at, revoked_at, user_type) FROM stdin;
\.


--
-- Data for Name: roles; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.roles (id, name, code, guard_name, created_at) FROM stdin;
1	Super Admin	super_admin	admin	2026-03-19 06:02:47.349713+00
2	Admin	admin	admin	2026-03-19 06:02:47.349713+00
4	Office Manager	office_manager	office	2026-03-19 06:02:47.349713+00
6	Finance	finance	admin	2026-03-19 06:02:48.040532+00
7	Support	support	admin	2026-03-19 06:02:48.052914+00
8	Viewer	viewer	admin	2026-03-19 06:02:48.053293+00
9	Office Finance	office_finance	office	2026-03-19 06:02:48.053325+00
10	Office Viewer	office_viewer	office	2026-03-19 06:02:48.053332+00
\.


--
-- Data for Name: transaction_settlements; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.transaction_settlements (id, financial_transaction_id, amount, created_at, created_by_admin_user_id, notes, quantity, settlement_date, settlement_mode, unit_amount) FROM stdin;
\.


--
-- Name: admin_users_id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public.admin_users_id_seq', 1, true);


--
-- Name: audit_logs_id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public.audit_logs_id_seq', 1, false);


--
-- Name: financial_transactions_id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public.financial_transactions_id_seq', 1, false);


--
-- Name: libraries_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public."libraries_Id_seq"', 1, false);


--
-- Name: library_accounts_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public."library_accounts_Id_seq"', 1, false);


--
-- Name: packages_id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public.packages_id_seq', 1, false);


--
-- Name: pos_devices_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public."pos_devices_Id_seq"', 1, false);


--
-- Name: qr_codes_id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public.qr_codes_id_seq', 1, false);


--
-- Name: refresh_tokens_id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public.refresh_tokens_id_seq', 1, false);


--
-- Name: roles_id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public.roles_id_seq', 10, true);


--
-- Name: transaction_settlements_id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public.transaction_settlements_id_seq', 1, false);


--
-- Name: __EFMigrationsHistory PK___EFMigrationsHistory; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."__EFMigrationsHistory"
    ADD CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId");


--
-- Name: admin_users PK_admin_users; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.admin_users
    ADD CONSTRAINT "PK_admin_users" PRIMARY KEY (id);


--
-- Name: audit_logs PK_audit_logs; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.audit_logs
    ADD CONSTRAINT "PK_audit_logs" PRIMARY KEY (id);


--
-- Name: financial_transactions PK_financial_transactions; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.financial_transactions
    ADD CONSTRAINT "PK_financial_transactions" PRIMARY KEY (id);


--
-- Name: libraries PK_libraries; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.libraries
    ADD CONSTRAINT "PK_libraries" PRIMARY KEY (id);


--
-- Name: library_accounts PK_library_accounts; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.library_accounts
    ADD CONSTRAINT "PK_library_accounts" PRIMARY KEY (id);


--
-- Name: packages PK_packages; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.packages
    ADD CONSTRAINT "PK_packages" PRIMARY KEY (id);


--
-- Name: pos_devices PK_pos_devices; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.pos_devices
    ADD CONSTRAINT "PK_pos_devices" PRIMARY KEY (id);


--
-- Name: qr_codes PK_qr_codes; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.qr_codes
    ADD CONSTRAINT "PK_qr_codes" PRIMARY KEY (id);


--
-- Name: refresh_tokens PK_refresh_tokens; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.refresh_tokens
    ADD CONSTRAINT "PK_refresh_tokens" PRIMARY KEY (id);


--
-- Name: roles PK_roles; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.roles
    ADD CONSTRAINT "PK_roles" PRIMARY KEY (id);


--
-- Name: transaction_settlements PK_transaction_settlements; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.transaction_settlements
    ADD CONSTRAINT "PK_transaction_settlements" PRIMARY KEY (id);


--
-- Name: IX_admin_users_email; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "IX_admin_users_email" ON public.admin_users USING btree (email);


--
-- Name: IX_admin_users_role_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_admin_users_role_id" ON public.admin_users USING btree (role_id);


--
-- Name: IX_admin_users_username; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "IX_admin_users_username" ON public.admin_users USING btree (username);


--
-- Name: IX_audit_logs_account_username; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_audit_logs_account_username" ON public.audit_logs USING btree (account_username);


--
-- Name: IX_audit_logs_account_username_trgm; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_audit_logs_account_username_trgm" ON public.audit_logs USING gin (account_username public.gin_trgm_ops);


--
-- Name: IX_audit_logs_action_name; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_audit_logs_action_name" ON public.audit_logs USING btree (action_name);


--
-- Name: IX_audit_logs_action_name_trgm; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_audit_logs_action_name_trgm" ON public.audit_logs USING gin (action_name public.gin_trgm_ops);


--
-- Name: IX_audit_logs_endpoint; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_audit_logs_endpoint" ON public.audit_logs USING btree (endpoint);


--
-- Name: IX_audit_logs_endpoint_trgm; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_audit_logs_endpoint_trgm" ON public.audit_logs USING gin (endpoint public.gin_trgm_ops);


--
-- Name: IX_audit_logs_ip_address; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_audit_logs_ip_address" ON public.audit_logs USING btree (ip_address);


--
-- Name: IX_audit_logs_ip_address_trgm; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_audit_logs_ip_address_trgm" ON public.audit_logs USING gin (ip_address public.gin_trgm_ops);


--
-- Name: IX_audit_logs_operation_date; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_audit_logs_operation_date" ON public.audit_logs USING btree (operation_date);


--
-- Name: IX_audit_logs_operation_date_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_audit_logs_operation_date_id" ON public.audit_logs USING btree (operation_date DESC, id DESC);


--
-- Name: IX_audit_logs_security_level; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_audit_logs_security_level" ON public.audit_logs USING btree (security_level);


--
-- Name: IX_audit_logs_status; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_audit_logs_status" ON public.audit_logs USING btree (status);


--
-- Name: IX_audit_logs_trace_identifier; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_audit_logs_trace_identifier" ON public.audit_logs USING btree (trace_identifier);


--
-- Name: IX_audit_logs_trace_identifier_trgm; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_audit_logs_trace_identifier_trgm" ON public.audit_logs USING gin (trace_identifier public.gin_trgm_ops);


--
-- Name: IX_financial_transactions_created_by_admin_user_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_financial_transactions_created_by_admin_user_id" ON public.financial_transactions USING btree (created_by_admin_user_id);


--
-- Name: IX_financial_transactions_created_by_library_account_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_financial_transactions_created_by_library_account_id" ON public.financial_transactions USING btree (created_by_library_account_id);


--
-- Name: IX_financial_transactions_library_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_financial_transactions_library_id" ON public.financial_transactions USING btree (library_id);


--
-- Name: IX_libraries_library_code; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "IX_libraries_library_code" ON public.libraries USING btree (library_code);


--
-- Name: IX_library_accounts_library_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_library_accounts_library_id" ON public.library_accounts USING btree (library_id);


--
-- Name: IX_library_accounts_role_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_library_accounts_role_id" ON public.library_accounts USING btree (role_id);


--
-- Name: IX_library_accounts_username; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "IX_library_accounts_username" ON public.library_accounts USING btree (username);


--
-- Name: IX_packages_added_by_admin_user_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_packages_added_by_admin_user_id" ON public.packages USING btree (added_by_admin_user_id);


--
-- Name: IX_packages_name; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "IX_packages_name" ON public.packages USING btree (name);


--
-- Name: IX_packages_package_code; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "IX_packages_package_code" ON public.packages USING btree (package_code);


--
-- Name: IX_pos_devices_activated_by_account_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_pos_devices_activated_by_account_id" ON public.pos_devices USING btree (activated_by_account_id);


--
-- Name: IX_pos_devices_library_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_pos_devices_library_id" ON public.pos_devices USING btree (library_id);


--
-- Name: IX_pos_devices_pos_code; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "IX_pos_devices_pos_code" ON public.pos_devices USING btree (pos_code);


--
-- Name: IX_qr_codes_created_by_library_account_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_qr_codes_created_by_library_account_id" ON public.qr_codes USING btree (created_by_library_account_id);


--
-- Name: IX_qr_codes_financial_transaction_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_qr_codes_financial_transaction_id" ON public.qr_codes USING btree (financial_transaction_id);


--
-- Name: IX_qr_codes_library_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_qr_codes_library_id" ON public.qr_codes USING btree (library_id);


--
-- Name: IX_qr_codes_package_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_qr_codes_package_id" ON public.qr_codes USING btree (package_id);


--
-- Name: IX_qr_codes_pos_device_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_qr_codes_pos_device_id" ON public.qr_codes USING btree (pos_device_id);


--
-- Name: IX_qr_codes_qr_reference; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "IX_qr_codes_qr_reference" ON public.qr_codes USING btree (qr_reference);


--
-- Name: IX_refresh_tokens_admin_user_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_refresh_tokens_admin_user_id" ON public.refresh_tokens USING btree (admin_user_id);


--
-- Name: IX_refresh_tokens_library_account_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_refresh_tokens_library_account_id" ON public.refresh_tokens USING btree (library_account_id);


--
-- Name: IX_refresh_tokens_token; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "IX_refresh_tokens_token" ON public.refresh_tokens USING btree (token);


--
-- Name: IX_roles_code; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "IX_roles_code" ON public.roles USING btree (code);


--
-- Name: IX_transaction_settlements_created_by_admin_user_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_transaction_settlements_created_by_admin_user_id" ON public.transaction_settlements USING btree (created_by_admin_user_id);


--
-- Name: IX_transaction_settlements_financial_transaction_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_transaction_settlements_financial_transaction_id" ON public.transaction_settlements USING btree (financial_transaction_id);


--
-- Name: admin_users FK_admin_users_roles_role_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.admin_users
    ADD CONSTRAINT "FK_admin_users_roles_role_id" FOREIGN KEY (role_id) REFERENCES public.roles(id) ON DELETE RESTRICT;


--
-- Name: financial_transactions FK_financial_transactions_admin_users_created_by_admin_user_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.financial_transactions
    ADD CONSTRAINT "FK_financial_transactions_admin_users_created_by_admin_user_id" FOREIGN KEY (created_by_admin_user_id) REFERENCES public.admin_users(id) ON DELETE RESTRICT;


--
-- Name: financial_transactions FK_financial_transactions_libraries_library_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.financial_transactions
    ADD CONSTRAINT "FK_financial_transactions_libraries_library_id" FOREIGN KEY (library_id) REFERENCES public.libraries(id) ON DELETE CASCADE;


--
-- Name: financial_transactions FK_financial_transactions_library_accounts_created_by_library_~; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.financial_transactions
    ADD CONSTRAINT "FK_financial_transactions_library_accounts_created_by_library_~" FOREIGN KEY (created_by_library_account_id) REFERENCES public.library_accounts(id) ON DELETE RESTRICT;


--
-- Name: library_accounts FK_library_accounts_libraries_library_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.library_accounts
    ADD CONSTRAINT "FK_library_accounts_libraries_library_id" FOREIGN KEY (library_id) REFERENCES public.libraries(id) ON DELETE CASCADE;


--
-- Name: library_accounts FK_library_accounts_roles_role_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.library_accounts
    ADD CONSTRAINT "FK_library_accounts_roles_role_id" FOREIGN KEY (role_id) REFERENCES public.roles(id) ON DELETE RESTRICT;


--
-- Name: packages FK_packages_admin_users_added_by_admin_user_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.packages
    ADD CONSTRAINT "FK_packages_admin_users_added_by_admin_user_id" FOREIGN KEY (added_by_admin_user_id) REFERENCES public.admin_users(id) ON DELETE RESTRICT;


--
-- Name: pos_devices FK_pos_devices_libraries_library_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.pos_devices
    ADD CONSTRAINT "FK_pos_devices_libraries_library_id" FOREIGN KEY (library_id) REFERENCES public.libraries(id) ON DELETE SET NULL;


--
-- Name: pos_devices FK_pos_devices_library_accounts_activated_by_account_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.pos_devices
    ADD CONSTRAINT "FK_pos_devices_library_accounts_activated_by_account_id" FOREIGN KEY (activated_by_account_id) REFERENCES public.library_accounts(id) ON DELETE SET NULL;


--
-- Name: qr_codes FK_qr_codes_financial_transactions_financial_transaction_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.qr_codes
    ADD CONSTRAINT "FK_qr_codes_financial_transactions_financial_transaction_id" FOREIGN KEY (financial_transaction_id) REFERENCES public.financial_transactions(id) ON DELETE RESTRICT;


--
-- Name: qr_codes FK_qr_codes_libraries_library_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.qr_codes
    ADD CONSTRAINT "FK_qr_codes_libraries_library_id" FOREIGN KEY (library_id) REFERENCES public.libraries(id) ON DELETE CASCADE;


--
-- Name: qr_codes FK_qr_codes_library_accounts_created_by_library_account_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.qr_codes
    ADD CONSTRAINT "FK_qr_codes_library_accounts_created_by_library_account_id" FOREIGN KEY (created_by_library_account_id) REFERENCES public.library_accounts(id) ON DELETE RESTRICT;


--
-- Name: qr_codes FK_qr_codes_packages_package_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.qr_codes
    ADD CONSTRAINT "FK_qr_codes_packages_package_id" FOREIGN KEY (package_id) REFERENCES public.packages(id) ON DELETE RESTRICT;


--
-- Name: qr_codes FK_qr_codes_pos_devices_pos_device_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.qr_codes
    ADD CONSTRAINT "FK_qr_codes_pos_devices_pos_device_id" FOREIGN KEY (pos_device_id) REFERENCES public.pos_devices(id) ON DELETE RESTRICT;


--
-- Name: refresh_tokens FK_refresh_tokens_admin_users_admin_user_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.refresh_tokens
    ADD CONSTRAINT "FK_refresh_tokens_admin_users_admin_user_id" FOREIGN KEY (admin_user_id) REFERENCES public.admin_users(id) ON DELETE CASCADE;


--
-- Name: refresh_tokens FK_refresh_tokens_library_accounts_library_account_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.refresh_tokens
    ADD CONSTRAINT "FK_refresh_tokens_library_accounts_library_account_id" FOREIGN KEY (library_account_id) REFERENCES public.library_accounts(id) ON DELETE CASCADE;


--
-- Name: transaction_settlements FK_transaction_settlements_admin_users_created_by_admin_user_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.transaction_settlements
    ADD CONSTRAINT "FK_transaction_settlements_admin_users_created_by_admin_user_id" FOREIGN KEY (created_by_admin_user_id) REFERENCES public.admin_users(id) ON DELETE RESTRICT;


--
-- Name: transaction_settlements FK_transaction_settlements_financial_transactions_financial_tr~; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.transaction_settlements
    ADD CONSTRAINT "FK_transaction_settlements_financial_transactions_financial_tr~" FOREIGN KEY (financial_transaction_id) REFERENCES public.financial_transactions(id) ON DELETE CASCADE;


--
-- PostgreSQL database dump complete
--

\unrestrict Hyb1su7qoyjjsSVIAtUrCH1avGKtbqWJ92XOog1uDVbwcDE5Gxi2kY5saHdjS1g

