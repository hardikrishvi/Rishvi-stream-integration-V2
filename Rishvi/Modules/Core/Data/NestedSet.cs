using Microsoft.EntityFrameworkCore;
using Rishvi.Modules.Core.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace Rishvi.Web.Modules.Core.Data
{
    public class GetNodeEntity
    {
        public Guid ParentId { get; set; }
        public int Left { get; set; }
        public int Right { get; set; }
    }

    public static class NestedSet
    {
        public static List<T> SqlQuery<T>(this SqlContext context, string query, Func<DbDataReader, T> map)
        {
            using (var command = context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = query;
                command.CommandType = CommandType.Text;

                //context.Database.OpenConnection();
                if (command.Connection.State == System.Data.ConnectionState.Closed)
                    command.Connection.Open();

                using (var result = command.ExecuteReader())
                {
                    var entities = new List<T>();

                    while (result.Read())
                    {
                        entities.Add(map(result));
                    }

                    return entities;
                }
            }
        }
        public static void BuildTree(this SqlContext context, string tableName, string extraCondition = null)
        {
            var extraWhereCondition = !string.IsNullOrEmpty(extraCondition) ? "AND " + extraCondition : "";

                var sqlQuery = string.Format(
                        @"WITH DbLevels AS
				(
					SELECT
					Id,
					CONVERT(VARCHAR(MAX), Id) AS thePath,
					1 AS Level
					FROM [{0}]
					WHERE ParentId IS NULL {1}
   
					UNION ALL
   
					SELECT
					e.Id,
					x.thePath + '.' + CONVERT(VARCHAR(MAX), e.Id) AS thePath,
						x.Level + 1 AS Level
					FROM DbLevels x 
						JOIN [{0}] e on e.ParentId = x.Id {1}
						),
					DbRows AS
					(
						SELECT
							DbLevels.*,
					ROW_NUMBER() OVER (ORDER BY thePath) AS Row
					FROM DbLevels
						)
					UPDATE
						[{0}]
					SET
					[{0}].[Left] = (ER.Row * 2) - ER.Level,
					[{0}].[Right] = ((ER.Row * 2) - ER.Level) + 
											   (
												   SELECT COUNT(*) * 2
					FROM DbRows ER2 
						WHERE ER2.thePath LIKE ER.thePath + '.%'
						) + 1
					FROM
						DbRows AS ER
					WHERE [{0}].Id = ER.Id;", tableName, extraWhereCondition);

                var result = SqlQuery(context, sqlQuery, x => new GetNodeEntity { }).FirstOrDefault();
        }

        public static void MoveToParentNode(this SqlContext context, string tableName, Guid currentNodeId, Guid? newParentNodeId = null, string extraCondition = null)
        {
            var extraWhereCondition = !string.IsNullOrEmpty(extraCondition) ? "AND " + extraCondition : "";

            // step 0: Initialize parameters.
            var sameParentIdExists = SqlQuery(context,
                $"SELECT ParentId FROM {tableName} Where Id = '{currentNodeId}' {extraWhereCondition};", x => new GetNodeEntity { ParentId = (Guid)x[0] }).First();

            if (sameParentIdExists != null && sameParentIdExists.ParentId == newParentNodeId)
                return;

            var leftCurrentNode = SqlQuery(context, $"SELECT [Left] FROM {tableName} Where Id = '{currentNodeId}' {extraWhereCondition};", x => new GetNodeEntity { Left = (int)x[0] }).First();
            var rightCurrentNode = SqlQuery(context, $"SELECT [Right] FROM {tableName} Where Id = '{currentNodeId}' {extraWhereCondition};", x => new GetNodeEntity { Right = (int)x[0] }).First();

            var rightParentNode = newParentNodeId != null
                ? SqlQuery(context, $"SELECT [Right] FROM {tableName} Where Id = '{newParentNodeId}' {extraWhereCondition};", x => new GetNodeEntity { Right = (int)x[0] }).First()
                : new GetNodeEntity { Right = 0 };

            var nodeSize = rightCurrentNode.Right - leftCurrentNode.Left + 1;

            // step 1: temporary "remove" moving node
            var up1 = $"UPDATE {tableName} SET [Left] = 0-([Left]), [Right] = 0-([Right]) WHERE [Left] >= {leftCurrentNode.Left} AND [Right] <= {rightCurrentNode.Right} {extraWhereCondition}";
            context.Database.ExecuteSqlRaw(up1);

            //step 2: decrease left and/or right position values of currently 'lower' AdminPermissionss (and parents)
            var up2 = $"UPDATE {tableName} SET [Left] = [Left] - {nodeSize} WHERE [Left] > {rightCurrentNode.Right} {extraWhereCondition}";
            context.Database.ExecuteSqlRaw(up2);
            var up3 = $"UPDATE {tableName} SET [Right] = [Right] - {nodeSize} WHERE [Right] > {rightCurrentNode.Right} {extraWhereCondition}";
            context.Database.ExecuteSqlRaw(up3);

            // step 3: increase left and/or right position values of future 'lower' AdminPermissionss (and parents)
            var up4 = $@"UPDATE {tableName}
					SET [Left] = [Left] + {nodeSize}
					WHERE [Left] >= CASE WHEN {rightParentNode.Right} > {rightCurrentNode.Right} THEN {rightParentNode.Right} - {nodeSize} ELSE {rightParentNode.Right} END {extraWhereCondition}";
            context.Database.ExecuteSqlRaw(up4);

            var up5 = $@"UPDATE {tableName}
					SET [Right] = [Right] + {nodeSize}
					WHERE [Right] >= CASE WHEN {rightParentNode.Right} > {rightCurrentNode.Right} THEN {rightParentNode.Right} - {nodeSize} ELSE {rightParentNode.Right} END {extraWhereCondition}";
            context.Database.ExecuteSqlRaw(up5);

            // step 4: move node (and it's subnodes)
            var up6 = $@"UPDATE {tableName}
					SET [Left] = 0-([Left]) + CASE WHEN {rightParentNode.Right} > {rightCurrentNode.Right} THEN {rightParentNode.Right} - {rightCurrentNode.Right} - 1 ELSE {rightParentNode.Right} - {rightCurrentNode.Right} - 1 + {nodeSize} END,
							  [Right] = 0-([Right]) + CASE WHEN {rightParentNode.Right} > {rightCurrentNode.Right} THEN {rightParentNode.Right} - {rightCurrentNode.Right} - 1 ELSE {rightParentNode.Right} - {rightCurrentNode.Right} - 1 + {nodeSize} END
					WHERE [Left] <= 0-{leftCurrentNode.Left} AND [Right] >= 0-{rightCurrentNode.Right} {extraWhereCondition}";
            context.Database.ExecuteSqlRaw(up6);

            // update it's parent AdminPermissions id
            var ParentId = newParentNodeId == null ? (object)"NULL" : newParentNodeId.ToString();
            var up7 = $"UPDATE {tableName} SET ParentId = '{ParentId}' WHERE Id = '{currentNodeId}' {extraWhereCondition}";
            context.Database.ExecuteSqlRaw(up7);
        }

        internal sealed class TempDeleteDataEntity
        {
            public int NewLeft { get; set; }
            public int NewRight { get; set; }
            public int HasLeafs { get; set; }
            public int Width { get; set; }
            public string SuperiorParent { get; set; }
        }

        public static void DeleteNode(this SqlContext context, string tableName, Guid nodeId, string extraCondition = null)
        {
            var extraWhereCondition = !string.IsNullOrEmpty(extraCondition) ? "AND " + extraCondition : "";

            var tempDeleteDataEntity = SqlQuery(context,
                $@"SELECT [Left] AS NewLeft, [Right] As NewRight, ([Right] - [Left] - 1) as HasLeafs, ([Right] - [Left] + 1) as Width, ParentId As SuperiorParent
						FROM {tableName} WHERE Id = '{nodeId}'", x => new TempDeleteDataEntity
                {
                    NewLeft = (int)x[0],
                    NewRight = (int)x[1],
                    HasLeafs = (int)x[2],
                    Width = (int)x[3],
                    SuperiorParent = x[4] == DBNull.Value ? null : ((Guid)x[4]).ToString()
                }).First();

            var del1 = $"DELETE FROM {tableName} WHERE Id = '{nodeId}' {extraWhereCondition}";
            context.Database.ExecuteSqlRaw(del1);

            if (tempDeleteDataEntity.HasLeafs == 0)
            {
                var del2 = $"DELETE FROM {tableName} WHERE [Left] BETWEEN {tempDeleteDataEntity.NewLeft} AND {tempDeleteDataEntity.NewRight} {extraWhereCondition}";
                context.Database.ExecuteSqlRaw(del2);

                var up1 = $"UPDATE {tableName} SET [Right] = [Right] - {tempDeleteDataEntity.Width} WHERE [Right] > {tempDeleteDataEntity.NewRight} {extraWhereCondition}";
                context.Database.ExecuteSqlRaw(up1);

                var up2 = $"UPDATE {tableName} SET [Left] = [Left] - {tempDeleteDataEntity.Width} WHERE [Left] > {tempDeleteDataEntity.NewRight} {extraWhereCondition}";
                context.Database.ExecuteSqlRaw(up2);
            }
            else
            {
                var del3 = $"DELETE FROM {tableName} WHERE [Left] = {tempDeleteDataEntity.NewLeft} {extraWhereCondition}";
                context.Database.ExecuteSqlRaw(del3);

                var SuperiorParent = tempDeleteDataEntity.SuperiorParent == null ? (object)"NULL" : tempDeleteDataEntity.SuperiorParent.ToString();
                var up3 = $@"UPDATE {tableName} SET [Right] = [Right] - 1, [Left] = [Left] - 1, ParentId = {SuperiorParent} 
							WHERE [Left] BETWEEN {tempDeleteDataEntity.NewLeft} AND {tempDeleteDataEntity.NewRight} {extraWhereCondition}";
                context.Database.ExecuteSqlRaw(up3);

                var up4 = $"UPDATE {tableName} SET [Right] = [Right] - 2 WHERE [Right] > {tempDeleteDataEntity.NewRight} {extraWhereCondition}";
                context.Database.ExecuteSqlRaw(up4);

                var up5 = $"UPDATE {tableName} SET [Left] = [Left] - 2 WHERE [Left] > {tempDeleteDataEntity.NewRight} {extraWhereCondition}";
                context.Database.ExecuteSqlRaw(up5);
            }
        }
    }
}