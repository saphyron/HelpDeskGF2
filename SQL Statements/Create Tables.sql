use [HelpDeskGF2]
go

-- Drop phase
if OBJECT_ID('dbo.ThreadResponses', 'U') is not null drop table dbo.ThreadResponses
if OBJECT_ID('dbo.Threads', 'U')         is not null drop table dbo.Threads
if OBJECT_ID('dbo.Users', 'U')           is not null drop table dbo.Users
go

-- Users
create table dbo.Users (
    UserId      int identity(1,1) not null primary key,
    Username    nvarchar(64)      not null,
    Password    nvarchar(64)      not null,
    Role        nvarchar(16)      not null default('user'),
    Name        nvarchar(128),

    constraint CK_User_Role
        check (Role in ('admin', 'user', 'guest'))
)
go

-- Threads
create table dbo.Threads (
    ThreadId            int identity(1,1) not null primary key,
    Title               nvarchar(300)      not null,
    CreatedByUserId     int                null,
    AnonymousName       nvarchar(64)       null,
    CreatedAt           datetime           not null default(getdate()),
    ThreadBody           nvarchar(max),
    Status               nvarchar(16) not null default('open'),

    constraint CK_Threads_Status
        check (Status in ('working on', 'open', 'closed')),

    -- FK kun hvis bruger findes
    constraint FK_Threads_Users
        foreign key (CreatedByUserId) references dbo.Users(UserId),

    -- Enten bruger ELLER anonym
    constraint CK_Threads_Author
        check (
            (CreatedByUserId is not null and AnonymousName is null)
            or
            (CreatedByUserId is null and AnonymousName is not null)
        )
)
go

-- Thread responses
create table dbo.ThreadResponses (
    ResponseId          int identity(1,1) not null primary key,
    ThreadId            int                not null,
    CreatedByUserId     int                null,
    AnonymousName       nvarchar(64)       null,
    CreatedAt           datetime           not null default(getdate()),
    ResponseBody        nvarchar(max)       not null,

    constraint FK_Response_Threads
        foreign key (ThreadId) references dbo.Threads(ThreadId),

    constraint FK_Response_Users
        foreign key (CreatedByUserId) references dbo.Users(UserId),

    constraint CK_Response_Author
        check (
            (CreatedByUserId is not null and AnonymousName is null)
            or
            (CreatedByUserId is null and AnonymousName is not null)
        )
)
go
