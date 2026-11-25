-- Alteracao aditiva para a base PostgreSQL do piloto.
-- Exigir backup verificado antes de executar. Nao usar database update do EF Core.
-- O mes historico e derivado da data existente; novos registros podem divergir.
BEGIN;

ALTER TABLE public."tbDespesa"
    ADD COLUMN IF NOT EXISTS "MesReferencia" date;

UPDATE public."tbDespesa"
SET "MesReferencia" = date_trunc('month', "Data")::date
WHERE "MesReferencia" IS NULL;

ALTER TABLE public."tbDespesa"
    ALTER COLUMN "MesReferencia" SET NOT NULL;

CREATE INDEX IF NOT EXISTS "IX_Despesa_UsuarioId_MesReferencia"
    ON public."tbDespesa" ("UsuarioId", "MesReferencia");

COMMIT;

-- Verificacao posterior: deve retornar zero.
SELECT COUNT(*) AS "DespesasSemMesReferencia"
FROM public."tbDespesa"
WHERE "MesReferencia" IS NULL;
