# Administração de usuários

A rota `/admin/usuarios` mostra os usuários cadastrados no YourMoney. A API
`GET /api/admin/usuarios` exige um JWT válido com o papel `Admin` e retorna
somente identificador, nome e e-mail, em páginas de até 100 registros. A
resposta usa `Cache-Control: no-store`. Senhas e hashes nunca entram no contrato.

O papel é concedido a uma conta existente com o script idempotente
`tools/postgres/grant-initial-admin.sql`. O script confere que há exatamente uma
conta com o e-mail indicado e altera apenas as tabelas de papéis do ASP.NET
Identity. Depois da concessão, o administrador deve entrar novamente para
receber o papel no JWT.

A tela só oferece “Enviar redefinição” quando `SMTP_HOST` e `SMTP_FROM` estão
configurados. O envio usa o fluxo existente de recuperação de senha e entrega
o link ao e-mail do próprio usuário. O administrador não recebe a senha nem o
token de redefinição.

O cadastro do Meu Pluggy é hospedado pela Pluggy/Auth0 e não é criado pela API
deste projeto. Para conectar contas financeiras de vários usuários dentro do
YourMoney, é necessária a integração regular do Pluggy Connect com credenciais
de aplicação apropriadas; a modalidade Meu Pluggy é voltada ao uso pessoal.
