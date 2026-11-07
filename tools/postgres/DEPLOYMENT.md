# YourMoney free pilot: Render + Neon

This runbook starts after the local PostgreSQL trial passes. No external service
has been created by this repository. Keep the notebook database and verified
backup available throughout the pilot.

## 1. Create accounts and destination

1. Create a Neon account and a PostgreSQL project in a nearby region. Review the
   current Free limits and whether the financial data size fits before import.
2. Save the database connection details in a private password manager. Require
   TLS (`SSL Mode=Require` for Npgsql; `PGSSLMODE=require` for psql).
3. Restore the verified SQL Server backup to a **separate** SQL Server database.
   Import that copy with pgloader into an empty Neon database using the procedure
   in [README.md](README.md). Store the pgloader load file outside the repository.
4. Run `after-pgloader.sql` and `verify-migration.ps1`. Stop if any table count,
   cent total or user reference differs. Do not use an incomplete destination.

## 2. Prepare secrets

1. Generate a new random JWT signing secret of at least 32 characters. Rotate
   the SQL Server password and old JWT signing secret that were previously in
   versioned configuration before making the repository public. Existing JWTs
   will expire after rotation.
2. Generate a password protected PFX certificate with a private key for ASP.NET
   Data Protection. Keep the PFX and password outside Git. Upload the PFX as a
   Render secret file, for example `/etc/secrets/yourmoney-dp.pfx`. Preserve it
   and its password across deployments; losing either makes newly protected
   values unreadable. Back it up in a private password manager or vault.
3. Configure Render environment variables:

   | Variable | Value |
   |---|---|
   | `DATABASE_PROVIDER` | `PostgreSQL` |
   | `DATABASE_CONNECTION_STRING` | Neon connection string with TLS |
   | `JwtSettings__Secret` | New random signing secret |
   | `DATA_PROTECTION_CERTIFICATE_PATH` | Render PFX secret file path |
   | `DATA_PROTECTION_CERTIFICATE_PASSWORD` | PFX password |
   | `ASPNETCORE_ENVIRONMENT` | `Production` |
   | `TLS_TERMINATED_BY_PROXY` | `true` (Render redirects HTTP to HTTPS) |
   | `Frontend__BaseUrl` | Public `https://...onrender.com` origin |

   Render supplies `PORT`; the API binds to it. Do not paste secret values into
   build arguments, Git, issue trackers or screenshots. Existing Pluggy and SMTP
   configuration, if used, also belongs in Render secrets.

## 3. Publish and check

1. Push the reviewed `YourMoney` repository to a private Git host. In Render,
   create a **Free Web Service** from the repository using its `Dockerfile`.
   Configure the secret file and variables above before the first deploy.
2. Wait for build and startup. Configuration errors must be explicit and must
   not include secret values. If startup fails, stop and correct the variables.
3. Open the assigned `*.onrender.com` URL. Verify the SPA, login with an existing
   account, dashboard totals, financial list, and one test write. Restart the
   service twice and repeat login and data checks. Compare the migration report.
4. Re-enter Pluggy credentials through the authenticated interface if that
   integration is needed. Windows DPAPI ciphertext cannot be moved as working
   Linux credentials. Test the integration only after re-entry.
5. Buy a domain only when desired. Add it in Render's custom domain settings,
   create the DNS records Render supplies at the registrar, then verify HTTPS.
   The `onrender.com` address works before a domain purchase.

## 4. Stop and recover

- Stop the switch if import verification, login, financial totals, restart
  persistence or HTTPS fails. Keep using the local notebook service while fixing
  the isolated destination. Never re-run pgloader into a populated destination;
  create a fresh database or reset only the isolated trial after verifying its
  exact identity.
- Do not apply the existing SQL Server EF migrations to PostgreSQL or run schema
  creation automatically. Future model changes need a PostgreSQL migration plan.
- Render Free spins down when idle and has an ephemeral filesystem. Financial
  records and key ring stay in Neon, but objective images currently stored under
  `App_Data/ObjectiveImages` would not survive a Render restart; avoid that upload
  feature in this pilot until external object storage is added.
- Review service and database free limits regularly. Do not treat free tier
  availability or backup retention as a production guarantee.
