CREATE TABLE cms_User(
	[AutoID] [int] IDENTITY(1,1) primary key,
	[UserName] [nvarchar](50) NOT NULL,
	[Gander] [nvarchar](2) NULL,
	[Age] [int] NULL
)