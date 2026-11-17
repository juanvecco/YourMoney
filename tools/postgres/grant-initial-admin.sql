-- Execute uma única vez no banco Identity do ambiente piloto, após confirmar
-- que o endereço pertence ao administrador. Reexecutar é seguro.
BEGIN;

DO $$
DECLARE
    admin_user_id text;
    admin_role_id text;
BEGIN
    IF (SELECT count(*) FROM public."AspNetUsers"
        WHERE "NormalizedEmail" = 'JUANVECCO@GMAIL.COM') <> 1 THEN
        RAISE EXCEPTION 'A conta de administrador precisa existir uma única vez.';
    END IF;

    SELECT "Id" INTO admin_user_id
    FROM public."AspNetUsers"
    WHERE "NormalizedEmail" = 'JUANVECCO@GMAIL.COM';

    SELECT "Id" INTO admin_role_id
    FROM public."AspNetRoles"
    WHERE "NormalizedName" = 'ADMIN';

    IF admin_role_id IS NULL THEN
        admin_role_id := gen_random_uuid()::text;
        INSERT INTO public."AspNetRoles" ("Id", "Name", "NormalizedName", "ConcurrencyStamp")
        VALUES (admin_role_id, 'Admin', 'ADMIN', gen_random_uuid()::text);
    END IF;

    INSERT INTO public."AspNetUserRoles" ("UserId", "RoleId")
    VALUES (admin_user_id, admin_role_id)
    ON CONFLICT DO NOTHING;
END $$;

COMMIT;
