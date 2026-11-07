-- Uma única mudança de padrão para todas as contas existentes no Neon.
-- Ausência de escolha passa a significar desligada no serviço de aplicação.
-- Este script desliga escolhas explícitas anteriormente ativadas sem apagar dados.
BEGIN;

UPDATE public."tbRegraNegocioUsuario"
SET "Ativa" = FALSE,
    "AtualizadaEm" = CURRENT_TIMESTAMP
WHERE "Ativa" = TRUE;

COMMIT;
