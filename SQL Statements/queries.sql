use [HelpDeskGF2]

select * from dbo.users

select userid, username, role, name from dbo.users order by userid

select * from dbo.threads
/*

SELECT
    dp.name   AS UserName,
    dr.name   AS RoleName
FROM sys.database_role_members drm
JOIN sys.database_principals dr ON drm.role_principal_id = dr.principal_id
JOIN sys.database_principals dp ON drm.member_principal_id = dp.principal_id
WHERE dp.name = 'John_Admin';


USE HelpDeskGF2;
GO
EXEC sp_helpuser 'John_Admin';


CREATE DATABASE HelpDeskGF2;
GO

CREATE LOGIN HDGF2 WITH PASSWORD = 'Password1!';
GO

USE HelpDeskGF2;
CREATE USER HDGF2 FOR LOGIN HDGF2;
ALTER ROLE db_owner ADD MEMBER HDGF2;
GO



SELECT name, type_desc
FROM sys.server_principals
WHERE name LIKE '%John%';
go
select name, type_desc
from sys.server_principals
where name = 'John_Admin';
go

SELECT @@SERVERNAME AS ServerName;
SELECT SYSTEM_USER, ORIGINAL_LOGIN();*/

SET IDENTITY_INSERT dbo.Users ON;
go
INSERT INTO dbo.Users
    (UserId, Username, Password, Role, Name, Hold, StudieRetning)
VALUES
    (0,'Guest', '', 'guest', 'Gæstebruger', 'Alle', 'Alle');
go
SET IDENTITY_INSERT dbo.Users OFF;
go

INSERT INTO dbo.Users
    (Username, Password, Role, Name, Hold, StudieRetning)
VALUES
    ('Admin', 'Password1!', 'admin', 'Adminbruger', 'Alle', 'Alle'),
    ('UserTest', 'Password1!', 'user', 'UserThatTestThings', 'Alle', 'Alle');
go

select * from dbo.users