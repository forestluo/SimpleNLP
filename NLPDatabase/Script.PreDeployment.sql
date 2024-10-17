/*
 预先部署脚本模板							
--------------------------------------------------------------------------------------
 此文件包含将在生成脚本之前执行的 SQL 语句。	
 使用 SQLCMD 语法将文件包含在预先部署脚本中。			
 示例:      :r .\myfile.sql								
 使用 SQLCMD 语法引用预先部署脚本中的变量。		
 示例:      :setvar TableName MyTable							
               SELECT * FROM [$(TableName)]					
--------------------------------------------------------------------------------------
*/

---开启所有服务器配置选项
EXEC sp_configure N'show advanced options', N'1' 
RECONFIGURE WITH OVERRIDE

--开启clr enabled 选项
EXEC sp_configure N'clr enabled', N'1'
RECONFIGURE WITH OVERRIDE

--关闭所有服务器配置选项
EXEC sp_configure N'show advanced options', N'0' 
RECONFIGURE WITH OVERRIDE

-- 声明临时变量
DECLARE @SqlHash AS BINARY(64);

-- 找到最近一行记录
SELECT TOP 1 @SqlHash = [hash] FROM sys.trusted_assemblies
WHERE [created_by] = 'MicrosoftAccount\forestluo@outlook.com' ORDER BY [create_date] DESC;
-- 检查结果
IF @@ROWCOUNT = 1
    EXEC sp_drop_trusted_assembly @SqlHash;

-- 生成Hash
SET @SqlHash =
(
	SELECT HASHBYTES('SHA2_512',
		(SELECT * FROM OPENROWSET (BULK 'E:\VisualStudioProjects\SimpleNLP\NLPDatabase\bin\Debug\NLPDatabase.dll', SINGLE_BLOB) AS [Data]))
);

-- 检查哈希
IF NOT EXISTS(SELECT TOP 1 * FROM sys.trusted_assemblies WHERE hash = @SqlHash)
BEGIN
    -- 增加信任代码
    EXEC sp_add_trusted_assembly @SqlHash
END