CREATE TABLE PasswordManager_Users(
   ID TEXT      PRIMARY KEY NOT NULL,
   EMAIL           TEXT NOT NULL,
   PASSWORDHASH    TEXT NOT NULL,
   SALT            TEXT NOT NULL,
   FirstName       TEXT NOT NULL,
   LastName        TEXT NOT NULL,
   EmailConfirmed  BIT NOT NULL,
   LockoutEnabled  BIT NOT NULL,
   LockoutEndDateUtc Date,
   AccessFailedCount INT NOT NULL,
   DateLastLogin TIMESTAMP NULL,
   DateLastLogout TIMESTAMP NULL
);

CREATE TABLE Roles(
    Id TEXT PRIMARY KEY NOT NULL,
    Name TEXT NOT NULL
);

CREATE TABLE UserRoles(
    USERID TEXT REFERENCES passwordmanager_users (ID) ON DELETE CASCADE,
    RoleId TEXT REFERENCES Roles (ID) ON DELETE CASCADE
);

CREATE TABLE PasswordManager_Accounts(
   ID TEXT NOT NULL,
   USERID TEXT NOT NULL,
   TITLE TEXT NOT NULL,
   USERNAME TEXT NOT NULL,
   PASSWORD TEXT NOT NULL,
   CREATED_AT TEXT,
   LAST_UPDATED_AT TEXT,
   PRIMARY KEY (ID, USERID)
);

CREATE TABLE UserTokens(
    Id TEXT PRIMARY KEY NOT NULL,
    LoginProvider TEXT NOT NULL,
    PROVIDERKEY TEXT NOT NULL,
    USERID TEXT REFERENCES passwordmanager_users (ID) ON DELETE CASCADE
);

drop table "usertokens"
drop table "passwordmanager_accounts";
drop table "roles";
drop table "userroles";
drop table "passwordmanager_users";
