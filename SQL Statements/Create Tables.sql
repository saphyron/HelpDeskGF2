use [HelpDeskGF2]
go

-- Drop phase
if OBJECT_ID('dbo.ThreadResponses', 'U')	is not null drop table dbo.ThreadResponses
if OBJECT_ID('dbo.Threads', 'U')			is not null drop table dbo.Threads
if OBJECT_ID('dbo.Users', 'U')				is not null drop table dbo.Users

-- Create phase
create table dbo.Users (
	UserId				int identity(1,1)	not null primary key,
	Username			nvarchar(64)		not null,
	Password			nvarchar(64)		not null,
	Role				nvarchar(16)		not null default('user'),
	Name				nvarchar(128)		,

	-- Constraints for role:
	constraint CK_User_Role check (Role in ('admin', 'user', 'guest')),
)
go

create table dbo.Threads (
	ThreadId			int identity(1,1)	not null primary key,
	Title				nvarchar(300)		not null,
	CreatedByUserId		int					not null,
	CreatedAt			Datetime			not null default(getdate()),
	ThreadBody			nvarchar(max)		,
	Status				nvarchar(16)		not null default('open'),

	-- Constraints for status:
	constraint CK_Threads_Status check (Status in ('working on', 'open', 'closed')),

	-- Foreign key for users:
	constraint FK_Threads_Users foreign key (CreatedByUserId) references dbo.Users(UserId)
)
go

create table dbo.ThreadResponses (
	ResponseId			int identity(1,1)	not null primary key,
	ThreadId			int					not null,
	CreatedByUserId		int					not null,
	CreatedAt			Datetime			not null default(getdate()),
	ResponseBody		nvarchar(max)		,

	-- Foreign key for Thread and User:
	constraint FK_Response_Users foreign key (CreatedByUserId) references dbo.Users(UserId),
	constraint FK_Response_Threads foreign key (ThreadId) references dbo.Threads(ThreadId)
)

