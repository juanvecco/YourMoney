-- Apply to the pilot PostgreSQL database before deploying account links.
-- This changes account mappings only; existing expenses and Open Finance data remain intact.
BEGIN;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint WHERE conname = 'AK_tbContaFinanceira_Id_UsuarioId'
          AND conrelid = 'public."tbContaFinanceira"'::regclass
    ) THEN
        ALTER TABLE public."tbContaFinanceira"
            ADD CONSTRAINT "AK_tbContaFinanceira_Id_UsuarioId" UNIQUE ("Id", "UsuarioId");
    END IF;
END $$;

CREATE TABLE IF NOT EXISTS public."tbPluggyAccountLink" (
    "Id" uuid PRIMARY KEY,
    "ConnectionId" uuid NOT NULL,
    "AccountId" varchar(128) NOT NULL,
    "ContaFinanceiraId" uuid NOT NULL,
    "UsuarioId" varchar(450) NOT NULL,
    CONSTRAINT "FK_PluggyAccountLink_Connection" FOREIGN KEY ("ConnectionId")
        REFERENCES public."tbPluggyConnection" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_PluggyAccountLink_Conta" FOREIGN KEY ("ContaFinanceiraId", "UsuarioId")
        REFERENCES public."tbContaFinanceira" ("Id", "UsuarioId") ON DELETE CASCADE
);

CREATE UNIQUE INDEX IF NOT EXISTS "IX_tbPluggyAccountLink_UsuarioId_ConnectionId_AccountId"
    ON public."tbPluggyAccountLink" ("UsuarioId", "ConnectionId", "AccountId");
CREATE INDEX IF NOT EXISTS "IX_tbPluggyAccountLink_ConnectionId"
    ON public."tbPluggyAccountLink" ("ConnectionId");
CREATE INDEX IF NOT EXISTS "IX_tbPluggyAccountLink_ContaFinanceiraId_UsuarioId"
    ON public."tbPluggyAccountLink" ("ContaFinanceiraId", "UsuarioId");
CREATE INDEX IF NOT EXISTS "IX_tbPluggyAccountLink_UsuarioId"
    ON public."tbPluggyAccountLink" ("UsuarioId");

COMMIT;
