-- Executar uma vez no Neon após publicar o padrão de Open Finance ligado.
-- Escolhas ausentes já usam o padrão ligado; esta atualização reativa escolhas
-- explícitas desligadas de contas existentes sem tocar em outros dados.
BEGIN;

UPDATE public."tbRegraNegocioUsuario"
SET "Ativa" = TRUE,
    "AtualizadaEm" = CURRENT_TIMESTAMP
WHERE "Codigo" = 'openfinance.visivel'
  AND "Ativa" = FALSE;

COMMIT;
