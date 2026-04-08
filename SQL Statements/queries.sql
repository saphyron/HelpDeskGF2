select * from dbo.users

select userid, username, role, name from dbo.users order by userid

select * from dbo.threads


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


SELECT name, type_desc
FROM sys.server_principals
WHERE name LIKE '%John%';
go
select name, type_desc
from sys.server_principals
where name = 'John_Admin';
go

SELECT @@SERVERNAME AS ServerName;
SELECT SYSTEM_USER, ORIGINAL_LOGIN();