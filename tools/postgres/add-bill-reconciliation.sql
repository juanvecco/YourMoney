-- Aditivo para a base PostgreSQL importada; revisar e aplicar antes da API atualizada.
-- Não executar migrations EF SQL Server no PostgreSQL.
BEGIN;

ALTER TABLE public."tbDespesa"
    ADD COLUMN IF NOT EXISTS "Conferida" boolean NOT NULL DEFAULT false;

CREATE TABLE IF NOT EXISTS public."tbFaturaConta" (
    "Id" uuid NOT NULL PRIMARY KEY,
    "IdContaFinanceira" uuid NOT NULL,
    "MesReferencia" timestamp without time zone NOT NULL,
    "ValorInformado" numeric(18,2) NOT NULL,
    "UsuarioId" character varying(450) NOT NULL,
    CONSTRAINT "FK_tbFaturaConta_tbContaFinanceira_IdContaFinanceira"
        FOREIGN KEY ("IdContaFinanceira") REFERENCES public."tbContaFinanceira" ("Id") ON DELETE CASCADE,
    CONSTRAINT "CK_tbFaturaConta_ValorInformado" CHECK ("ValorInformado" >= 0)
);

CREATE INDEX IF NOT EXISTS "IX_tbFaturaConta_IdContaFinanceira"
    ON public."tbFaturaConta" ("IdContaFinanceira");
CREATE INDEX IF NOT EXISTS "IX_tbFaturaConta_UsuarioId"
    ON public."tbFaturaConta" ("UsuarioId");
CREATE UNIQUE INDEX IF NOT EXISTS "IX_tbFaturaConta_UsuarioId_IdContaFinanceira_MesReferencia"
    ON public."tbFaturaConta" ("UsuarioId", "IdContaFinanceira", "MesReferencia");

COMMIT;
