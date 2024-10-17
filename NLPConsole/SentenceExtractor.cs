using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace NLP
{
    public class SentenceExtractor
    {
		// 句式模板
		// 不要调整顺序！！！
		private static readonly string[][] TEMPLATES =
		{
			// 符号嵌套
			new string[] {"“$", "$a", "(，|：|。|；|？|！)+‘$", "$b", "(。|；|？|！)*’$", "$c", "(。|？|！|…)+”$"},

			new string[] {"“$", "$a", "(，|：|。|；|？|！|…)*”$", "$b", "(，|：)?“$", "$c", "(。|？|！|…)+”$"},
			new string[] {"‘$", "$a", "(，|：|。|；|？|！|…)*’$", "$b", "(，|：)?‘$", "$c", "(。|？|！|…)+’$"},
			new string[] {"「$", "$a", "(，|：|。|；|？|！|…)*」$", "$b", "(，|：)?“$", "$c", "(。|？|！|…)+”$"},
			new string[] {"『$", "$a", "(，|：|。|；|？|！|…)*』$", "$b", "(，|：)?‘$", "$c", "(。|？|！|…)+’$"},
			new string[] {"〝$", "$a", "(，|：|。|；|？|！|…)*〞$", "$b", "(，|：)?“$", "$c", "(。|？|！|…)+”$"},
			new string[] {"【$", "$a", "(，|：|。|；|？|！|…)*】$", "$b", "(，|：)?‘$", "$c", "(。|？|！|…)+’$"},

			new string[] {"$a", "(，|：)?“$", "$b", "(，|；|…)*”$", "$c", "(。|？|！|…)+$"},
			new string[] {"$a", "(，|：)?‘$", "$b", "(，|；|…)*’$", "$c", "(。|？|！|…)+$"},
			new string[] {"$a", "(，|：)?「$", "$b", "(，|；|…)*」$", "$c", "(。|？|！|…)+$"},
			new string[] {"$a", "(，|：)?『$", "$b", "(，|；|…)*』$", "$c", "(。|？|！|…)+$"},
			new string[] {"$a", "(，|：)?〝$", "$b", "(，|；|…)*〞$", "$c", "(。|？|！|…)+$"},
			new string[] {"$a", "(，|：)?【$", "$b", "(，|；|…)*】$", "$c", "(。|？|！|…)+$"},

			new string[] {"“$", "$a", "(，|：|。|；|？|！)+‘$", "$b", "(。|；|？|！|…)*’”$"},
			new string[] {"“‘$", "$a", "(，|：|。|；|？|！|…)*’$", "$b", "(。|；|？|！|…)+”$"},

			new string[] {"“$", "$a", "(，|：|。|；|？|！|…)*”$", "$b", "(。|？|！|…)+$"},
			new string[] {"‘$", "$a", "(，|：|。|；|？|！|…)*’$", "$b", "(。|？|！|…)+$"},

			new string[] {"$a", "(，|：)?“$", "$b", "(。|？|！|…)?”$"},
			new string[] {"$a", "(，|：)?‘$", "$b", "(。|？|！|…)?’$"},
			new string[] {"$a", "(，|：)?「$", "$b", "(。|？|！|…)?」$"},
			new string[] {"$a", "(，|：)?『$", "$b", "(。|？|！|…)?』$"},
			new string[] {"$a", "(，|：)?〝$", "$b", "(。|？|！|…)?〞$"},
			new string[] {"$a", "(，|：)?【$", "$b", "(。|？|！|…)?】$"},

			// 常见符号
			new string[] {"$a", "(：)$", "$b", "(。|；|？|！|…)+$"},
			new string[] {"$a", "(，|：)?“$", "$b", "(。|；|？|！|…)*”$"},
			new string[] {"$a", "(，|：)?‘$", "$b", "(。|；|？|！|…)*’$"},
			new string[] {"$a", "(，|：)?（$", "$b", "(。|；|？|！|…)*）$"},
			new string[] {"$a", "(，|：)?「$", "$b", "(。|；|？|！|…)*」$"},
			new string[] {"$a", "(，|：)?『$", "$b", "(。|；|？|！|…)*』$"},
			new string[] {"$a", "(，|：)?〝$", "$b", "(。|；|？|！|…)*〞$"},

			new string[] {"$a", "(，|：)?《$", "$b", "》(。|？|！|…)+$"},
			new string[] {"$a", "(，|：)?【$", "$b", "】(。|？|！|…)+$"},

			// 符号嵌套
			new string[] {"“‘$", "$a", "(，|：|。|；|？|！|…)*’”$"},
			new string[] {"“（$", "$a", "(，|：|。|；|？|！|…)*）”$"},

			// 常见符号
			new string[] {"“$", "$a", "(，|：|。|；|？|！|…)*”$"},
			new string[] {"‘$", "$a", "(，|：|。|；|？|！|…)*’$"},
			new string[] {"（$", "$a", "(，|：|。|；|？|！|…)*）$"},
			new string[] {"《$", "$a", "(，|：|。|；|？|！|…)*》$"},
			new string[] {"【$", "$a", "(，|：|。|；|？|！|…)*】$"},
			// 比较少见
			new string[] {"「$", "$a", "(，|：|。|；|？|！|…)*」$"},
			new string[] {"『$", "$a", "(，|：|。|；|？|！|…)*』$"},
			new string[] {"〖$", "$a", "(，|：|。|；|？|！|…)*〗$"},
			new string[] {"〝$", "$a", "(，|：|。|；|？|！|…)*〞$"},

			// 最简单的句子
			new string[] {"$a", "(。|；|？|！|…)+$"},
		};

#if _USE_CLR
		private static string[] GetTemplate(int index)
#elif _USE_CONSOLE
		private static string[]? GetTemplate(int index)
#endif
		{
			// 返回结果
			return index >= 0 &&
				index < TEMPLATES.Length ? TEMPLATES[index] : null;
		}

		private static int GetMatched(string[] input)
		{
#if DEBUG
			Debug.Assert(input != null && input.Length > 0);
#endif
			// 索引
			int index = -1;
			// 循环处理
			for (int i = 0; i < TEMPLATES.Length; i++)
			{
				// 获得模板
				string[] template = TEMPLATES[i];
				// 检查结果
				if (input.Length < template.Length) continue;

				// 索引参数
				int j = 0;
				// 循环处理
				for (;j < template.Length; j++)
				{
					// 检查起始字符
					if (template[j][0] == '$')
					{
						// 检查起始字符
						if (input[j][0] != '$') break;
					}
					else
					{
						// 检查起始字符
						if (input[j][0] == '$') break;
						// 获得匹配结果
						Match match =
							Regex.Match(input[j], template[j]);
						// 检查匹配结果
						if (!match.Success || match.Index != 0) break;
					}
				}
				// 检查结果
				if (j >= template.Length) { index = i; break; }
			}
			// 返回结果
			return index;
		}

#if _USE_CLR
		private static string[][] Extract(string[] input)
#elif _USE_CONSOLE
		private static string[][]? Extract(string[] input)
#endif
		{
#if DEBUG
			Debug.Assert(input != null && input.Length > 0);
#endif
			// 创建对象
			List<string[]> sentences = new List<string[]>();
			// 循环处理
			for (int i = 0; i < input.Length; i++)
			{
				// 创建新输入
				string[] newInput =
					new string[input.Length - i];
				// 拷贝
				for (int j = 0;
					j < newInput.Length; j++)
				{
					// 拷贝数组
					newInput[j] = input[i + j];
				}
				// 获得模板索引
				int index = GetMatched(newInput);
				// 检查结果
				if (index >= 0)
				{
					// 创建对象
					List<string> items = new List<string>();

#if _USE_CLR
					// 获得模板                         
					string[] template = GetTemplate(index);
#elif _USE_CONSOLE
					// 获得模板                         
					string[]? template = GetTemplate(index);
#endif
#if DEBUG
					Debug.Assert(template != null);
#endif
					// 将头部数据装入
					for (int j = 0;
						j < template.Length; j++) items.Add(newInput[j]);
					// 增加索引
					i += template.Length - 1;
					// 添加句子
					sentences.Add(items.ToArray());
				}
			}
			// 返回结果
			return sentences.Count > 0 ? sentences.ToArray() : null;
		}

#if _USE_CLR
		public static string[] Extract(string content)
#elif _USE_CONSOLE
		public static string[]? Extract(string? content)
#endif
		{
			// 检查结果
			if (string.IsNullOrEmpty(content)) return null;
			// 清理内容
			content = MiscTool.ClearContent(content);
			// 检查结果
			if (string.IsNullOrEmpty(content)) return null;

			// 切分字符串
			string[] output = SentenceSpliter.Split(content);
			// 检查结果
			if (output == null || output.Length <= 0) return null;

			// 合并字符串
			output = SentenceSpliter.MergeContent(output);
			// 检查结果
			if (output == null || output.Length <= 0) return null;

#if _USE_CLR
			// 获得拆分结果                   
			string[][] result = Extract(output);
#elif _USE_CONSOLE
			// 获得拆分结果                   
			string[][]? result = Extract(output);
#endif
			// 检查结果
			if (result == null || result.Length <= 0) return null;

			// 创建链表
			List<string> sentences = new List<string>();
			// 增加字符串
			foreach (string[] sentence in result)
			{
				// 拼接字符串
				string value = SentenceSpliter.Concatenate(sentence);
				// 检查结果
				if (value != null && value.Length > 0) sentences.Add(value);
			}
			// 返回结果
			return sentences.ToArray();
		}

#if _USE_CONSOLE
		public static void Main(string[] args)
		{
			int fid = -1;
			// 尝试解析
			if (!int.TryParse(args[1], out fid))
			{
				Console.WriteLine("请输入正确的参数！"); return;
			}

			// 获得最大ID号码
			int maxID = FilteredContent.GetMaxID();
			// 打印信息
			Console.WriteLine("\tmax id = " + maxID);
			// 生成随机数
			Random random = new Random(fid + (int)DateTime.Now.Ticks);

			do
			{
				// 获得内容                     
				string? content =
					FilteredCache.LoadContent(fid);
				// 检查内容
				if (string.IsNullOrEmpty(content))
				{
					Console.WriteLine("RawContent.GetContent返回空字符串！");
				}
				else
                {
					// 打印原始内容
					Console.WriteLine("----------------------------------------");
					Console.WriteLine("打印原始内容！");
					Console.WriteLine("----------------------------------------");
					Console.WriteLine(content);

					// 清理内容
					content = MiscTool.ClearContent(content);
					// 检查内容
					if (string.IsNullOrEmpty(content))
					{
						Console.WriteLine("MiscTool.ClearContent返回空字符串！");
					}
					else
                    {
						// 打印清洗后的内容
						Console.WriteLine("----------------------------------------");
						Console.WriteLine("打印ClearContent内容！");
						Console.WriteLine("----------------------------------------");
						Console.WriteLine(content);

						// 参数
						string[] output;

						// 切分字符串
						output = SentenceSpliter.Split(content);
						// 打印结果
						Console.WriteLine("----------------------------------------");
						Console.WriteLine("打印Split内容！");
						Console.WriteLine("----------------------------------------");
						// 打印结果
						foreach (string item in output) Console.WriteLine(item);

						// 合并字符串
						output = SentenceSpliter.MergeContent(output);
						// 打印结果
						Console.WriteLine("----------------------------------------");
						Console.WriteLine("打印MergeContent内容！");
						Console.WriteLine("----------------------------------------");
						// 打印结果
						foreach (string item in output) Console.WriteLine(item);

						// 获得句子
						string[][]? sentences = Extract(output);
						// 打印结果
						Console.WriteLine("----------------------------------------");
						Console.WriteLine("打印ExtractSentences内容！");
						Console.WriteLine("----------------------------------------");
						// 检查结果
						if (sentences == null || sentences.Length <= 0)
						{
							Console.WriteLine("未能找到可以匹配的句式模板！！");
						}
						else
						{
							// 循环处理
							foreach (string[] sentence in sentences)
							{
								// 打印结果
								Console.WriteLine(SentenceSpliter.Concatenate(sentence));
								Console.WriteLine("----------------------------------------");
							}
						}
					}
				}

				// 随机往下
				fid += random.Next(1, 10);
				// 等待键盘
				if (Console.ReadKey().Key == ConsoleKey.Escape) break;

			} while (true);
		}
#endif
	}
}
