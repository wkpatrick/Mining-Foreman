CREATE TABLE IF NOT EXISTS public.SchemaVersions
(
    SchemaVersionKey serial PRIMARY KEY,
    SchemaName       text    NOT NULL,
    SchemaVersion    integer NOT NULL,
    CreateDate       timestamp DEFAULT now(),
    UpdateDate       timestamp DEFAULT now()
);
DO
$$
    BEGIN
        IF NOT EXISTS(SELECT 1 FROM SchemaVersions) THEN
            --Setup triggers for table
            CREATE OR REPLACE FUNCTION update_schema_date()
                RETURNS trigger AS
            $BODY$
            BEGIN
                UPDATE SchemaVersions SET UpdateDate = now() WHERE SchemaVersionKey = NEW.SchemaVersionKey;
                RETURN new;
            END;
            $BODY$ LANGUAGE plpgsql;

            CREATE TRIGGER schema_versions_update_date
                AFTER UPDATE OF SchemaVersion
                ON SchemaVersions
                FOR EACH ROW
            EXECUTE PROCEDURE update_schema_date();

            CREATE TABLE public.Users
            (
                UserKey                serial PRIMARY KEY,
                CharacterId            int       NOT NULL,
                CharacterName          text      NOT NULL,
                AccessToken            text      NOT NULL,
                RefreshToken           text      NOT NULL,
                RefreshTokenExpiresUTC timestamp NOT NULL,
                APIToken               text,
                CreateDate             timestamp DEFAULT now(),
                UpdateDate             timestamp DEFAULT now()
            );

            CREATE OR REPLACE FUNCTION update_user_date()
                RETURNS trigger AS
            $BODY$
            BEGIN
                UPDATE Users SET UpdateDate = now() WHERE UserKey = NEW.UserKey;
                RETURN new;
            END;
            $BODY$ LANGUAGE plpgsql;

            CREATE TRIGGER user_update_date
                AFTER UPDATE OF CharcterId, AccessToken, RefreshToken, ExpiresUTC
                ON Users
                FOR EACH ROW
            EXECUTE PROCEDURE update_user_date();

            CREATE TABLE public.MiningLedger
            (
                MiningLedgerKey serial PRIMARY KEY,
                UserKey         int  NOT NULL,
                Date            date NOT NULL,
                Quantity        int  NOT NULL,
                SolarSystemId   int  NOT NULL,
                TypeId          int  NOT NULL,
                CreateDate      timestamp DEFAULT now(),
                UpdateDate      timestamp DEFAULT now()
            );

            CREATE OR REPLACE FUNCTION update_mining_ledger_date()
                RETURNS trigger AS
            $BODY$
            BEGIN
                UPDATE MiningLedger SET UpdateDate = now() WHERE MiningLedgerKey = NEW.MiningLedgerKey;
                RETURN new;
            END;
            $BODY$ LANGUAGE plpgsql;

            CREATE TRIGGER mining_ledger_update_date
                AFTER UPDATE OF MiningLedgerKey, UserKey, Quantity
                ON MiningLedger
                FOR EACH ROW
            EXECUTE PROCEDURE update_mining_ledger_date();

            CREATE TABLE public.MiningFleets
            (
                MiningFleetKey serial PRIMARY KEY,
                FleetBossKey   int NOT NULL,
                StartTime      timestamp,
                EndTime        timestamp,
                IsActive       bool
            );


            CREATE TABLE public.MiningFleetMembers
            (
                MiningFleetMemberKey serial PRIMARY KEY,
                MiningFleetKey       int  NOT NULL,
                UserKey              int  NOT NULL,
                IsActive             bool NOT NULL DEFAULT false
            );

            --TODO: Switch FleetKey to MiningFleetKey to match the rest of the schema
            CREATE TABLE public.MiningFleetLedger
            (
                MiningFleetLedgerKey serial PRIMARY KEY,
                FleetKey             int  NOT NULL,
                UserKey              int  NOT NULL,
                Date                 date NOT NULL,
                Quantity             int  NOT NULL,
                SolarSystemId        int  NOT NULL,
                TypeId               int  NOT NULL,
                IsStartingLedger     bool NOT NULL,
                LedgerCount          int  NOT NULL DEFAULT 0,
                CreateDate           timestamp     DEFAULT now(),
                UpdateDate           timestamp     DEFAULT now()
            );

            CREATE TABLE public.PendingMiningLedger
            (
                PendingMiningLedgerKey serial PRIMARY KEY,
                FleetKey               int NOT NULL,
                MemberKey              int
            );

        END IF;
    END
$$;
