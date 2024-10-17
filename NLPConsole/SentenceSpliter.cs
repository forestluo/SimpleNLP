using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace NLP
{
    public class SentenceSpliter
    {
		public static string[] Split(string content)
		{
#if DEBUG
			Debug.Assert(!string.IsNullOrEmpty(content));
#endif
			// 链表
			List<string> segments = new List<string>();
			// 循环处理
			for (int i = 0; i < content.Length;)
			{
				// 获得字符
				char charValue = content[i];
				// 检查是否为标点符号
				if (Punctuation.IsPunctuation(charValue))
				{
					// 增加索引
					i++;
					// 加入字符
					segments.Add(charValue.ToString());
				}
				else
				{
					// 创建字符串
					StringBuilder sb = new StringBuilder();
					// 加入标记位
					sb.Append('$');
					// 加入字符
					sb.Append(charValue);
					// 循环处理
					for (++i; i < content.Length; i++)
					{
						// 获得字符
						charValue = content[i];
						// 检查是否为标点符号
						if (Punctuation.IsPunctuation(charValue)) break;
						// 加入字符
						sb.Append(charValue);
					}
					// 加入字符串
					segments.Add(sb.ToString());
				}
			}
			// 返回结果
			return AdjustSegments(segments.ToArray());
		}

		public static string[] ExSplit(string content)
		{
#if DEBUG
			Debug.Assert(!string.IsNullOrEmpty(content));
#endif

#if _USE_CLR
			// 提取数量词
			FunctionSegment[] fses =
				QuantityExtractor.Extract(content);
#elif _USE_CONSOLE
			// 提取数量词
			FunctionSegment[]? fses =
				QuantityExtractor.Extract(content);
#endif
			// 检查结果
			if (fses == null ||
				fses.Length <= 0) return Split(content);

			// 链表
			List<string> segments = new List<string>();
			// 循环处理
			for (int i = 0; i < content.Length;)
			{
#if _USE_CLR
				// 检查是否在数量词范围内
				FunctionSegment fs =
					FunctionSegment.GetIncluded(fses, i);
#elif _USE_CONSOLE
				// 检查是否在数量词范围内
				FunctionSegment? fs =
					FunctionSegment.GetIncluded(fses, i);
#endif
				// 检查结果
				if (fs != null)
				{
					// 加入标记
					segments.Add(string.
						Format("#{0}", fs.Value));
					// 增加索引
					i += fs.Length; continue;
				}

				// 获得字符
				char charValue = content[i];
				// 检查是否为标点符号
				if (Punctuation.IsPunctuation(charValue))
				{
					// 增加索引
					i++;
					// 加入字符
					segments.Add(charValue.ToString());
				}
				else
				{
					// 创建字符串
					StringBuilder sb = new StringBuilder();
					// 加入标记位
					sb.Append('$');
					// 加入字符
					sb.Append(charValue);
					// 循环处理
					for (++i; i < content.Length; i++)
					{
						// 获得字符
						charValue = content[i];
						// 检查是否为标点符号
						if (Punctuation.IsPunctuation(charValue)) break;
						// 检查是否在数量词范围内
						if (FunctionSegment.GetIncluded(fses, i) != null) break;
						// 加入字符
						sb.Append(charValue);
					}
					// 加入字符串
					segments.Add(sb.ToString());
				}
			}
			// 返回结果
			return AdjustSegments(segments.ToArray());
		}

		private static string[] AdjustSegments(string[] input)
		{
#if DEBUG
			Debug.Assert(input != null && input.Length > 0);
#endif
			// 链表
			List<string> segments = new List<string>();
			// 循环处理
			for (int i = 1; i < input.Length; i++)
			{
				if (input[i][0] == '…' &&
					input[i - 1][0] != '$') input[i] = "$…";
			}
			// 清理结果
			segments.Clear();
			// 循环处理
			for (int i = 0; i < input.Length; i++)
			{
				// 获得字符
				char cValue = input[i][0];
				// 检查类型
				if (cValue == '$' ||
					i >= input.Length - 1)
				{
					// 增加子符串
					segments.Add(input[i]); continue;
				}
				// 获得字符
				char cNext = input[i + 1][0];
				// 检查结果
				if (cNext == '$')
				{
					// 增加子符串
					segments.Add(input[i]); continue;
				}
				// 检查字符
				if (!((Punctuation.IsNEndMajorSplitter(cValue)
						&& Punctuation.IsNarStartSplitter(cNext)) ||
					(Punctuation.IsMajorSplitter(cValue)
						&& Punctuation.IsNarEndSplitter(cNext))))
				{
					// 增加子符串
					segments.Add(input[i]); continue;
				}
				// 创建子符串
				StringBuilder sb = new StringBuilder();
				// 增加字符
				sb.Append(cValue); sb.Append(cNext);
				// 增加子符串
				segments.Add(sb.ToString()); i++;
			}
			// 返回结果
			return segments.ToArray();
		}

		public static string Concatenate(string[] input)
		{
#if DEBUG
			Debug.Assert(input != null && input.Length > 0);
#endif
			// 创建对象
			StringBuilder sb = new StringBuilder();
			// 循环处理
			foreach (string s in input)
			{
				// 增加字符串
				sb.Append(s[0] == '$' ? s.Substring(1) : s);
			}
			// 返回结果
			return sb.ToString();
		}

		public static string[] MergeContent(string[] input)
		{
			// 定义参数
			string[] output = input;

#if DEBUG
			Debug.Assert(input != null && input.Length > 0);
#endif

			// 定义参数
			bool merged = false;

			do
			{
				// 设置参数
				merged = false;

				// 设置输入参数
				input = output;
				// 合并字符串
				output = MergeString(input);

				// 检查结果
				if (output.Length < input.Length) merged = true;

				// 设置参数
				input = output;
				// 合并引用串
				output = MergeQuotation(input);

				// 检查结果
				if (output.Length < input.Length) merged = true;

				// 设置参数
				input = output;
				// 合并引用串
				output = MergeSegment(input);

				// 检查结果
				if (output.Length < input.Length) merged = true;

				// 设置参数
				input = output;
				// 合并引用串
				output = MergeCompound(input);

				// 检查结果
				if (output.Length < input.Length) merged = true;

			} while (merged);
			// 返回结果
			return output;
		}

		private static string[] MergeString(string[] input)
		{
			// 输出参数
			string[] output = input;
#if DEBUG
			Debug.Assert(input != null && input.Length > 0);
#endif

			do
			{
				// 设置输入参数
				input = output;
				// 链表
				List<string> segments = new List<string>();
				// 循环处理
				int index = 0;
				while (index < input.Length - 1)
				{
					// 检查类型
					if (input[index][0] != '$' ||
						input[index + 1][0] != '$')
					{
						// 增加字符串
						segments.Add(input[index]); index++;
					}
					else
					{
						// 创建字符串
						StringBuilder sb = new StringBuilder();
						// 增加字符串
						sb.Append(input[index]);
						sb.Append(input[index + 1].Substring(1));
						segments.Add(sb.ToString()); index += 2;
					}
				}
				// 末尾字符
				for (; index < input.Length; index++) segments.Add(input[index]);

				// 设置输出结果
				output = segments.ToArray();

			} while (output.Length < input.Length);

			// 返回结果
			return output;
		}

		private static string[] MergeQuotation(string[] input)
		{
			// 输出参数
			string[] output = input;
#if DEBUG
			Debug.Assert(input != null && input.Length > 0);
#endif

			do
			{
				// 设置输入参数
				input = output;
				// 链表
				List<string> segments = new List<string>();
				// 循环处理
				int index = 0;
				while (index < input.Length - 2)
				{
					// 检查参数
					if (input[index][0] == '$' ||
						input[index + 1][0] != '$' ||
							input[index + 2][0] == '$')
					{
						// 增加字符串
						segments.Add(input[index]); index++;
					}
					else
					{
						// 匹配参数
						Match match;

						// 匹配字符
						match = Regex.Match(input[index],
							string.Format("({0})$", Punctuation.GetPairStarts()));
						// 检查结果
						if (!match.Success || match.Index != 0)
						{
							// 增加字符串
							segments.Add(input[index]); index++;
						}
						else
						{
							// 获得结尾字符
							char cEnd = Punctuation.GetPairEnd(match.Value[0]);
							// 继续匹配
							match = Regex.Match(input[index + 2], string.Format("^{0}", cEnd));
							// 检查结果
							if (!match.Success || match.Index != 0)
							{
								// 增加字符串
								segments.Add(input[index]); index++;
							}
							else
							{
								// 创建字符串
								StringBuilder sb = new StringBuilder();
								// 增加字符串
								sb.Append('$');
								sb.Append(input[index]);
								sb.Append(input[index + 1].Substring(1));
								sb.Append(input[index + 2][0]);
								segments.Add(sb.ToString());

								// 检查长度
								if (input[index + 2].Length <= 1)
								{
									// 增加索引
									index += 3;
								}
								else
								{
									// 修改项目
									input[index + 2] = input[index + 2].Substring(1); index += 2;
								}
							}
						}
					}
				}
				// 末尾字符
				for (; index < input.Length; index++) segments.Add(input[index]);

				// 设置输出结果
				output = segments.ToArray();

			} while (output.Length < input.Length);

			// 返回结果
			return output;
		}

		private static string[] MergeSegment(string[] input)
		{
			// 输出参数
			string[] output = input;
#if DEBUG
			Debug.Assert(input != null && input.Length > 0);
#endif

			do
			{
				// 设置输入参数
				input = output;

				// 链表
				List<string> segments = new List<string>();
				// 循环处理
				int index = 0;
				while (index < input.Length - 2)
				{
					// 检查参数
					if (input[index][0] != '$')
					{
						// 增加内容
						segments.Add(input[index]); index++;
					}
					else
					{
						// 尝试匹配
						Match match =
							Regex.Match(input[index + 1],
							string.Format("({0})+$", Punctuation.GetNEndMajorSplitters()));
						// 检查参数
						if (!match.Success || match.Index != 0)
						{
							// 增加内容
							segments.Add(input[index]); index++;
						}
						else
						{
							// 检查参数
							if (input[index + 2][0] != '$')
							{
								// 增加内容
								segments.Add(input[index]); index++;
							}
							else
							{
								// 创建字符串
								StringBuilder sb = new StringBuilder();
								// 增加字符串
								sb.Append(input[index]);
								sb.Append(input[index + 1]);
								sb.Append(input[index + 2].Substring(1));
								segments.Add(sb.ToString()); index += 3;
							}
						}
					}
				}
				// 末尾字符
				for (; index < input.Length; index++) segments.Add(input[index]);

				// 设置输出结果
				output = segments.ToArray();

			} while (output.Length < input.Length);

			// 返回结果
			return output;
		}

		private static string[] MergeCompound(string[] input)
		{
			// 输出参数
			string[] output = input;
#if DEBUG
			Debug.Assert(input != null && input.Length > 0);
#endif

			do
			{
				// 设置输入参数
				input = output;

				// 链表
				List<string> segments = new List<string>();
				// 增加内容
				segments.Add(input[0]);
				// 循环处理
				int index = 1;
				while (index < input.Length - 2)
				{
					// 检查字符
					if (input[index][0] != '$')
					{
						// 增加内容
						segments.Add(input[index]); index++;
					}
					else
					{
						// 尝试匹配
						Match match =
							Regex.Match(input[index + 1],
							string.Format("({0})+$", Punctuation.GetEndMajorSplitters()));
						// 检查参数
						if (!match.Success || match.Index != 0)
						{
							// 增加内容
							segments.Add(input[index]); index++;
						}
						else
						{
							// 检查参数
							if (input[index + 2][0] != '$')
							{
								// 增加内容
								segments.Add(input[index]); index++;
							}
							else
							{
								// 获得字符
								char cLast = input[index - 1][input[index - 1].Length - 1];
								// 检查结果
								if (input[index - 1][0] == '$' ||
									!Punctuation.IsPairStart(cLast))
								{
									// 增加内容
									segments.Add(input[index]); index++;
								}
								else
								{
									// 创建字符串
									StringBuilder sb = new StringBuilder();
									// 增加字符串
									sb.Append(input[index]);
									sb.Append(input[index + 1]);
									sb.Append(input[index + 2].Substring(1));
									segments.Add(sb.ToString()); index += 3;
								}
							}
						}
					}
				}
				// 末尾字符
				for (; index < input.Length; index++) segments.Add(input[index]);

				// 设置输出结果
				output = segments.ToArray();

			} while (output.Length < input.Length);

			// 返回结果
			return output;
		}
	}
}
