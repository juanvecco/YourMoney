-- Aditivo para a base PostgreSQL importada do SQL Server.
-- Revisar em ambiente de ensaio antes de aplicar ao Neon; não executar as migrations EF SQL Server.
BEGIN;

CREATE TABLE IF NOT EXISTS public."tbRegraNegocioUsuario" (
    "Id" uuid NOT NULL PRIMARY KEY,
    "Codigo" character varying(100) NOT NULL,
    "Ativa" boolean NOT NULL,
    "AtualizadaEm" timestamp with time zone NOT NULL,
    "UsuarioId" character varying(450) NOT NULL
);

CREATE INDEX IF NOT EXISTS "IX_tbRegraNegocioUsuario_UsuarioId"
    ON public."tbRegraNegocioUsuario" ("UsuarioId");

CREATE UNIQUE INDEX IF NOT EXISTS "IX_tbRegraNegocioUsuario_UsuarioId_Codigo"
    ON public."tbRegraNegocioUsuario" ("UsuarioId", "Codigo");

COMMIT;
