using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

/*
 *
 * Copyright (c) 2022-2023 Carbon Community 
 * All rights reserved.
 *
 */

namespace Oxide.Ext.SQLite;

public class Sql
{
	internal object[] _argsFinal;
	internal Sql _rhs;
	internal string _sqlFinal;

	private static readonly Regex RxParams = new Regex("(?<!@)@\\w+", RegexOptions.Compiled);

	internal readonly object[] _args;
	internal readonly string _sql;

	public class SqlJoinClause
	{
		public SqlJoinClause(Sql sql) { _sql = sql; }

		internal readonly Sql _sql;

		public Sql On(string onClause, params object[] args)
		{
			return _sql.Append("ON " + onClause, args);
		}
	}

	public Sql() { }
	public Sql(string sql, params object[] args)
	{
		_sql = sql;
		_args = args;
	}

	public static Sql Builder => new Sql();

	public string SQL { get { Build(); return _sqlFinal; } }
	public object[] Arguments { get { Build(); return _argsFinal; } }

	private void Build()
	{
		if (_sqlFinal != null) return;

		var stringBuilder = new StringBuilder();
		var list = new List<object>();

		Build(stringBuilder, list, null);

		_sqlFinal = stringBuilder.ToString();
		_argsFinal = list.ToArray();
	}
	public Sql Append(Sql sql)
	{
		if (_rhs != null) _rhs.Append(sql); else _rhs = sql;

		return this;
	}
	public Sql Append(string sql, params object[] args)
	{
		return Append(new Sql(sql, args));
	}
	private static bool Is(Sql sql, string sqltype)
	{
		return sql != null && sql._sql != null && sql._sql.StartsWith(sqltype, StringComparison.InvariantCultureIgnoreCase);
	}
	private void Build(StringBuilder sb, List<object> args, Sql lhs)
	{
		if (!string.IsNullOrEmpty(_sql))
		{
			if (sb.Length > 0)
			{
				sb.Append("\n");
			}

			var text = ProcessParams(_sql, _args, args);

			if (Is(lhs, "WHERE ") && Is(this, "WHERE "))
			{
				text = "AND " + text.Substring(6);
			}

			if (Is(lhs, "ORDER BY ") && Is(this, "ORDER BY "))
			{
				text = ", " + text.Substring(9);
			}
			sb.Append(text);
		}

		if (_rhs == null)
		{
			return;
		}

		_rhs.Build(sb, args, this);
	}
	public Sql Where(string sql, params object[] args)
	{
		return Append(new Sql("WHERE (" + sql + ")", args));
	}
	public Sql OrderBy(params object[] columns)
	{
		return Append(new Sql("ORDER BY " + string.Join(", ", (from x in columns
															   select x.ToString()).ToArray<string>()), Array.Empty<object>()));
	}
	public Sql Select(params object[] columns)
	{
		return Append(new Sql("SELECT " + string.Join(", ", (from x in columns
															 select x.ToString()).ToArray<string>()), Array.Empty<object>()));
	}
	public Sql From(params object[] tables)
	{
		return Append(new Sql("FROM " + string.Join(", ", (from x in tables
														   select x.ToString()).ToArray<string>()), Array.Empty<object>()));
	}
	public Sql GroupBy(params object[] columns)
	{
		return Append(new Sql("GROUP BY " + string.Join(", ", (from x in columns
															   select x.ToString()).ToArray<string>()), Array.Empty<object>()));
	}
	private SqlJoinClause Join(string joinType, string table)
	{
		return new Sql.SqlJoinClause(Append(new Sql(joinType + table, Array.Empty<object>())));
	}
	public SqlJoinClause InnerJoin(string table)
	{
		return Join("INNER JOIN ", table);
	}
	public SqlJoinClause LeftJoin(string table)
	{
		return Join("LEFT JOIN ", table);
	}
	public static string ProcessParams(string sql, object[] argsSrc, List<object> argsDest)
	{
		return RxParams.Replace(sql, delegate (Match m)
		{
			var text = m.Value.Substring(1);

			var obj = (object)null;
			if (int.TryParse(text, out var num))
			{
				if (num < 0 || num >= argsSrc.Length)
				{
					throw new ArgumentOutOfRangeException(string.Format("Parameter '@{0}' specified but only {1} parameters supplied (in `{2}`)", num, argsSrc.Length, sql));
				}
				obj = argsSrc[num];
			}
			else
			{
				bool flag = false;
				obj = null;
				foreach (object obj2 in argsSrc)
				{
					PropertyInfo property = obj2.GetType().GetProperty(text);
					if (!(property == null))
					{
						obj = property.GetValue(obj2, null);
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					throw new ArgumentException($"Parameter '@{text}' specified but none of the passed arguments have a property with this name (in '{sql}')");
				}
			}
			if (obj is IEnumerable && !(obj is string) && !(obj is byte[]))
			{
				var stringBuilder = new StringBuilder();

				foreach (object item in (obj as IEnumerable))
				{
					stringBuilder.Append(((stringBuilder.Length == 0) ? "@" : ",@") + argsDest.Count.ToString());
					argsDest.Add(item);
				}

				return stringBuilder.ToString();
			}

			argsDest.Add(obj);
			return $"@{argsDest.Count - 1}";
		});
	}
	public static void AddParams(IDbCommand cmd, object[] items, string parameterPrefix)
	{
		foreach (object item in items)
		{
			AddParam(cmd, item, "@");
		}
	}
	public static void AddParam(IDbCommand cmd, object item, string parameterPrefix)
	{
		var dbDataParameter = item as IDbDataParameter;
		if (dbDataParameter != null)
		{
			dbDataParameter.ParameterName = string.Format("{0}{1}", parameterPrefix, cmd.Parameters.Count);
			cmd.Parameters.Add(dbDataParameter);
			return;
		}
		var dbDataParameter2 = cmd.CreateParameter();
		dbDataParameter2.ParameterName = string.Format("{0}{1}", parameterPrefix, cmd.Parameters.Count);

		if (item == null)
		{
			dbDataParameter2.Value = DBNull.Value;
		}
		else
		{
			var type = item.GetType();
			if (type.IsEnum)
			{
				dbDataParameter2.Value = (int)item;
			}
			else if (type == typeof(Guid))
			{
				dbDataParameter2.Value = item.ToString();
				dbDataParameter2.DbType = DbType.String;
				dbDataParameter2.Size = 40;
			}
			else if (type == typeof(string))
			{
				dbDataParameter2.Size = Math.Max(((string)item).Length + 1, 4000);
				dbDataParameter2.Value = item;
			}
			else if (type == typeof(bool))
			{
				dbDataParameter2.Value = (((bool)item) ? 1 : 0);
			}
			else
			{
				dbDataParameter2.Value = item;
			}
		}
		cmd.Parameters.Add(dbDataParameter2);
	}
}
