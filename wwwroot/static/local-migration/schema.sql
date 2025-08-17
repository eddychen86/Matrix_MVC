IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [Hashtags] (
    [TagId] uniqueidentifier NOT NULL,
    [Content] nvarchar(10) NOT NULL,
    [Status] int NOT NULL,
    CONSTRAINT [PK_Hashtags] PRIMARY KEY ([TagId])
);
GO

CREATE TABLE [Users] (
    [UserId] uniqueidentifier NOT NULL,
    [Role] int NOT NULL,
    [UserName] nvarchar(50) NOT NULL,
    [Email] nvarchar(100) NOT NULL,
    [Password] nvarchar(max) NOT NULL,
    [Country] nvarchar(max) NULL,
    [Gender] int NULL,
    [CreateTime] datetime2 NOT NULL,
    [LastLoginTime] datetime2 NULL,
    [Status] int NOT NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY ([UserId])
);
GO

CREATE TABLE [Persons] (
    [PersonId] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [DisplayName] nvarchar(50) NULL,
    [Bio] nvarchar(300) NULL,
    [AvatarPath] nvarchar(2048) NULL,
    [BannerPath] nvarchar(2048) NULL,
    [IsPrivate] int NOT NULL,
    [WalletAddress] nvarchar(max) NULL,
    [ModifyTime] datetime2 NULL,
    CONSTRAINT [PK_Persons] PRIMARY KEY ([PersonId]),
    CONSTRAINT [FK_Persons_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE CASCADE
);
GO

CREATE TABLE [Articles] (
    [ArticleId] uniqueidentifier NOT NULL,
    [AuthorId] uniqueidentifier NOT NULL,
    [Content] nvarchar(4000) NOT NULL,
    [IsPublic] int NOT NULL,
    [Status] int NOT NULL,
    [CreateTime] datetime2 NOT NULL,
    [PraiseCount] int NOT NULL,
    [CollectCount] int NOT NULL,
    CONSTRAINT [PK_Articles] PRIMARY KEY ([ArticleId]),
    CONSTRAINT [FK_Articles_Persons_AuthorId] FOREIGN KEY ([AuthorId]) REFERENCES [Persons] ([PersonId]) ON DELETE CASCADE
);
GO

CREATE TABLE [Follows] (
    [FollowId] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [FollowedId] uniqueidentifier NOT NULL,
    [Type] int NOT NULL,
    [FollowTime] datetime2 NOT NULL,
    CONSTRAINT [PK_Follows] PRIMARY KEY ([FollowId]),
    CONSTRAINT [FK_Follows_Persons_UserId] FOREIGN KEY ([UserId]) REFERENCES [Persons] ([PersonId]) ON DELETE NO ACTION
);
GO

CREATE TABLE [Friendships] (
    [FriendshipId] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [FriendId] uniqueidentifier NOT NULL,
    [Status] int NOT NULL,
    [RequestDate] datetime2 NOT NULL,
    CONSTRAINT [PK_Friendships] PRIMARY KEY ([FriendshipId]),
    CONSTRAINT [FK_Friendships_Persons_FriendId] FOREIGN KEY ([FriendId]) REFERENCES [Persons] ([PersonId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Friendships_Persons_UserId] FOREIGN KEY ([UserId]) REFERENCES [Persons] ([PersonId]) ON DELETE NO ACTION
);
GO

CREATE TABLE [LoginRecords] (
    [LoginId] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [IpAddress] nvarchar(max) NOT NULL,
    [UserAgent] nvarchar(max) NOT NULL,
    [LoginTime] datetime2 NOT NULL,
    [History] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_LoginRecords] PRIMARY KEY ([LoginId]),
    CONSTRAINT [FK_LoginRecords_Persons_UserId] FOREIGN KEY ([UserId]) REFERENCES [Persons] ([PersonId]) ON DELETE CASCADE
);
GO

CREATE TABLE [Notifications] (
    [NotifyId] uniqueidentifier NOT NULL,
    [GetId] uniqueidentifier NOT NULL,
    [SendId] uniqueidentifier NOT NULL,
    [Type] int NOT NULL,
    [IsRead] int NOT NULL,
    [SentTime] datetime2 NOT NULL,
    [IsReadTime] datetime2 NULL,
    CONSTRAINT [PK_Notifications] PRIMARY KEY ([NotifyId]),
    CONSTRAINT [FK_Notifications_Persons_GetId] FOREIGN KEY ([GetId]) REFERENCES [Persons] ([PersonId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Notifications_Persons_SendId] FOREIGN KEY ([SendId]) REFERENCES [Persons] ([PersonId]) ON DELETE NO ACTION
);
GO

CREATE TABLE [Reports] (
    [ReportId] uniqueidentifier NOT NULL,
    [ReporterId] uniqueidentifier NOT NULL,
    [TargetId] uniqueidentifier NOT NULL,
    [Type] int NOT NULL,
    [Reason] nvarchar(500) NOT NULL,
    [Status] int NOT NULL,
    [ResolverId] uniqueidentifier NULL,
    [ProcessTime] datetime2 NULL,
    CONSTRAINT [PK_Reports] PRIMARY KEY ([ReportId]),
    CONSTRAINT [FK_Reports_Persons_ReporterId] FOREIGN KEY ([ReporterId]) REFERENCES [Persons] ([PersonId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Reports_Persons_ResolverId] FOREIGN KEY ([ResolverId]) REFERENCES [Persons] ([PersonId]) ON DELETE NO ACTION
);
GO

CREATE TABLE [ArticleAttachments] (
    [FileId] uniqueidentifier NOT NULL,
    [ArticleId] uniqueidentifier NOT NULL,
    [FilePath] nvarchar(max) NOT NULL,
    [Type] nvarchar(max) NOT NULL,
    [FileName] nvarchar(max) NULL,
    [MimeType] nvarchar(max) NULL,
    CONSTRAINT [PK_ArticleAttachments] PRIMARY KEY ([FileId]),
    CONSTRAINT [FK_ArticleAttachments_Articles_ArticleId] FOREIGN KEY ([ArticleId]) REFERENCES [Articles] ([ArticleId]) ON DELETE CASCADE
);
GO

CREATE TABLE [ArticleHashtags] (
    [ArticleId] uniqueidentifier NOT NULL,
    [TagId] uniqueidentifier NOT NULL,
    CONSTRAINT [PK_ArticleHashtags] PRIMARY KEY ([ArticleId], [TagId]),
    CONSTRAINT [FK_ArticleHashtags_Articles_ArticleId] FOREIGN KEY ([ArticleId]) REFERENCES [Articles] ([ArticleId]) ON DELETE CASCADE,
    CONSTRAINT [FK_ArticleHashtags_Hashtags_TagId] FOREIGN KEY ([TagId]) REFERENCES [Hashtags] ([TagId]) ON DELETE CASCADE
);
GO

CREATE TABLE [PraiseCollects] (
    [EventId] uniqueidentifier NOT NULL,
    [Type] int NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [ArticleId] uniqueidentifier NOT NULL,
    [CreateTime] datetime2 NOT NULL,
    CONSTRAINT [PK_PraiseCollects] PRIMARY KEY ([EventId]),
    CONSTRAINT [FK_PraiseCollects_Articles_ArticleId] FOREIGN KEY ([ArticleId]) REFERENCES [Articles] ([ArticleId]) ON DELETE CASCADE,
    CONSTRAINT [FK_PraiseCollects_Persons_UserId] FOREIGN KEY ([UserId]) REFERENCES [Persons] ([PersonId]) ON DELETE NO ACTION
);
GO

CREATE TABLE [Replies] (
    [ReplyId] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [ArticleId] uniqueidentifier NOT NULL,
    [Content] nvarchar(1000) NOT NULL,
    [ReplyTime] datetime2 NOT NULL,
    CONSTRAINT [PK_Replies] PRIMARY KEY ([ReplyId]),
    CONSTRAINT [FK_Replies_Articles_ArticleId] FOREIGN KEY ([ArticleId]) REFERENCES [Articles] ([ArticleId]) ON DELETE CASCADE,
    CONSTRAINT [FK_Replies_Persons_UserId] FOREIGN KEY ([UserId]) REFERENCES [Persons] ([PersonId]) ON DELETE NO ACTION
);
GO

CREATE INDEX [IX_ArticleAttachments_ArticleId] ON [ArticleAttachments] ([ArticleId]);
GO

CREATE INDEX [IX_ArticleHashtags_TagId] ON [ArticleHashtags] ([TagId]);
GO

CREATE INDEX [IX_Articles_AuthorId] ON [Articles] ([AuthorId]);
GO

CREATE INDEX [IX_Follows_UserId] ON [Follows] ([UserId]);
GO

CREATE INDEX [IX_Friendships_FriendId] ON [Friendships] ([FriendId]);
GO

CREATE INDEX [IX_Friendships_UserId] ON [Friendships] ([UserId]);
GO

CREATE INDEX [IX_LoginRecords_UserId] ON [LoginRecords] ([UserId]);
GO

CREATE INDEX [IX_Notifications_GetId] ON [Notifications] ([GetId]);
GO

CREATE INDEX [IX_Notifications_SendId] ON [Notifications] ([SendId]);
GO

CREATE UNIQUE INDEX [IX_Persons_UserId] ON [Persons] ([UserId]);
GO

CREATE INDEX [IX_PraiseCollects_ArticleId] ON [PraiseCollects] ([ArticleId]);
GO

CREATE INDEX [IX_PraiseCollects_UserId] ON [PraiseCollects] ([UserId]);
GO

CREATE INDEX [IX_Replies_ArticleId] ON [Replies] ([ArticleId]);
GO

CREATE INDEX [IX_Replies_UserId] ON [Replies] ([UserId]);
GO

CREATE INDEX [IX_Reports_ReporterId] ON [Reports] ([ReporterId]);
GO

CREATE INDEX [IX_Reports_ResolverId] ON [Reports] ([ResolverId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250717174655_InitialCreate', N'8.0.11');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250719125504_SetDefaultValueForPrimaryKeys', N'8.0.11');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Users]') AND [c].[name] = N'UserId');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Users] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [Users] ADD DEFAULT (NEWID()) FOR [UserId];
GO

DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Reports]') AND [c].[name] = N'ReportId');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Reports] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [Reports] ADD DEFAULT (NEWID()) FOR [ReportId];
GO

DECLARE @var2 sysname;
SELECT @var2 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Replies]') AND [c].[name] = N'ReplyId');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Replies] DROP CONSTRAINT [' + @var2 + '];');
ALTER TABLE [Replies] ADD DEFAULT (NEWID()) FOR [ReplyId];
GO

DECLARE @var3 sysname;
SELECT @var3 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PraiseCollects]') AND [c].[name] = N'EventId');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [PraiseCollects] DROP CONSTRAINT [' + @var3 + '];');
ALTER TABLE [PraiseCollects] ADD DEFAULT (NEWID()) FOR [EventId];
GO

DECLARE @var4 sysname;
SELECT @var4 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Persons]') AND [c].[name] = N'PersonId');
IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [Persons] DROP CONSTRAINT [' + @var4 + '];');
ALTER TABLE [Persons] ADD DEFAULT (NEWID()) FOR [PersonId];
GO

DECLARE @var5 sysname;
SELECT @var5 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Notifications]') AND [c].[name] = N'NotifyId');
IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [Notifications] DROP CONSTRAINT [' + @var5 + '];');
ALTER TABLE [Notifications] ADD DEFAULT (NEWID()) FOR [NotifyId];
GO

DECLARE @var6 sysname;
SELECT @var6 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[LoginRecords]') AND [c].[name] = N'LoginId');
IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [LoginRecords] DROP CONSTRAINT [' + @var6 + '];');
ALTER TABLE [LoginRecords] ADD DEFAULT (NEWID()) FOR [LoginId];
GO

DECLARE @var7 sysname;
SELECT @var7 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Hashtags]') AND [c].[name] = N'TagId');
IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [Hashtags] DROP CONSTRAINT [' + @var7 + '];');
ALTER TABLE [Hashtags] ADD DEFAULT (NEWID()) FOR [TagId];
GO

DECLARE @var8 sysname;
SELECT @var8 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Friendships]') AND [c].[name] = N'FriendshipId');
IF @var8 IS NOT NULL EXEC(N'ALTER TABLE [Friendships] DROP CONSTRAINT [' + @var8 + '];');
ALTER TABLE [Friendships] ADD DEFAULT (NEWID()) FOR [FriendshipId];
GO

DECLARE @var9 sysname;
SELECT @var9 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Follows]') AND [c].[name] = N'FollowId');
IF @var9 IS NOT NULL EXEC(N'ALTER TABLE [Follows] DROP CONSTRAINT [' + @var9 + '];');
ALTER TABLE [Follows] ADD DEFAULT (NEWID()) FOR [FollowId];
GO

DECLARE @var10 sysname;
SELECT @var10 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ArticleAttachments]') AND [c].[name] = N'FileId');
IF @var10 IS NOT NULL EXEC(N'ALTER TABLE [ArticleAttachments] DROP CONSTRAINT [' + @var10 + '];');
ALTER TABLE [ArticleAttachments] ADD DEFAULT (NEWID()) FOR [FileId];
GO

DECLARE @var11 sysname;
SELECT @var11 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Articles]') AND [c].[name] = N'ArticleId');
IF @var11 IS NOT NULL EXEC(N'ALTER TABLE [Articles] DROP CONSTRAINT [' + @var11 + '];');
ALTER TABLE [Articles] ADD DEFAULT (NEWID()) FOR [ArticleId];
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250719133848_AddUuidDefaultValues', N'8.0.11');
GO

COMMIT;
GO

