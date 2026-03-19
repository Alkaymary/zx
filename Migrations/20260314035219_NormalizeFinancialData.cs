using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyApi.Migrations
{
    /// <inheritdoc />
    public partial class NormalizeFinancialData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                update financial_transactions
                set transaction_type = 'openinvoice'
                where lower(transaction_type) = 'balance';
                """);

            migrationBuilder.Sql("""
                update transaction_settlements
                set settlement_mode = 'PartialAmount'
                where settlement_mode is null or settlement_mode = '';
                """);

            migrationBuilder.Sql("""
                update transaction_settlements
                set settlement_date = created_at
                where settlement_date <= timestamp with time zone '1900-01-01 00:00:00+00';
                """);

            migrationBuilder.Sql("""
                with settlement_totals as (
                    select financial_transaction_id, coalesce(sum(amount), 0) as settled_amount
                    from transaction_settlements
                    group by financial_transaction_id
                )
                update financial_transactions ft
                set paid_amount = coalesce(st.settled_amount, 0),
                    remaining_amount = greatest(ft.amount - coalesce(st.settled_amount, 0), 0),
                    status = case
                        when ft.status = 'cancelled' then 'cancelled'
                        when greatest(ft.amount - coalesce(st.settled_amount, 0), 0) = 0 and coalesce(st.settled_amount, 0) > 0 then 'paid'
                        when coalesce(st.settled_amount, 0) > 0 then 'partiallypaid'
                        else 'open'
                    end,
                    updated_at = now()
                from settlement_totals st
                where ft.id = st.financial_transaction_id;
                """);

            migrationBuilder.Sql("""
                update financial_transactions
                set paid_amount = 0,
                    remaining_amount = amount,
                    status = case when status = 'cancelled' then 'cancelled' else 'open' end,
                    updated_at = now()
                where lower(transaction_type) = 'openinvoice'
                  and id not in (select distinct financial_transaction_id from transaction_settlements);
                """);

            migrationBuilder.Sql("""
                delete from financial_transactions
                where lower(transaction_type) = 'payment';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                update financial_transactions
                set transaction_type = 'balance'
                where id = 10;
                """);
        }
    }
}
