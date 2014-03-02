using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace ExpressionSample
{
    [TestFixture]
    partial class Parser
    {
    }

    public enum NodeType
    {
        None = 0,
        Operator,
        Function,
        Constant,
        Item
    };

    [TestFixture]
    public partial class Node
    {


        public string Expression;
        public Node Left = null;
        public Node Right = null;
        //public bool Function = false;
        public NodeType Type = NodeType.None;

        // NUnit用コンストラクタ
        public Node()
        {
        }

        // コンストラクタ
        public Node(string expression)
        {
            this.Expression = RemoveBrackets(expression.Trim());
        }

        // 解析
        public void Parse()
        {
            int pos = GetOperatorPosition((this.Expression));
            if (pos == 0)
            {
                this.Left       = null;
                this.Right      = new Node(this.Expression.Substring(pos + 1));
                this.Right.Parse();
                this.Expression = this.Expression[pos].ToString();
                this.Type       = NodeType.Operator;
            }else
            if (pos > 0)
            {
                this.Left       = new Node(this.Expression.Substring(0, pos));
                this.Left.Parse();
                this.Right      = new Node(this.Expression.Substring(pos+1));
                this.Right.Parse();
                this.Expression = this.Expression [ pos ].ToString();
                this.Type       = NodeType.Operator;
            }
            else
            {
                if (IsFunction(this.Expression) == true)
                {
                    // 関数オペランドを左要素とする
                    this.Left = new Node(Regex.Replace(Expression, @"^(SUM|AVG)\((.+)\)$", "$2"));
                    this.Left.Parse();
                    // 関数名だけをExpressionとして保存する。本当は属性を何か持ちたい
                    this.Expression = (new Regex(@"^SUM|AVG")).Match(this.Expression).ToString();
                    this.Type = NodeType.Function;
                }
                else
                {
                    if((new Regex(@".*[^0-9].*")).IsMatch(this.Expression) )
                    {
                        this.Type   = NodeType.Item;
                    }else{
                        this.Type   = NodeType.Constant;
                    }
                }
            }
            return;
        }

        /// <summary>
        /// 演算子の位置を返す
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        /// <remarks>
        /// <para>一番優先度の低いものを返す</para>
        /// <para>（）の中はスキップする</para>
        /// </remarks>
        static public int GetOperatorPosition(string expression)
        {
            int pos = -1;
            var ope = 0;
            int nest = 0;

            for (int i=0; i<expression.Length; ++i)
            {
                {
                    if (expression [ i ] == ')')
                    {
                        nest--;
                    }else
                    if (expression [ i ] == '(')
                    {
                        nest++;
                    }

                    // カッコ内
                    if (nest > 0)
                    {
                        continue;
                    }

                    switch (expression [ i ])
                    {
                        case ',':
                            if (ope == ',')
                            {
                                continue;
                            }
                            break;
                        case '*':
                        case '/':
                            if (ope != 0)
                            {
                                continue;
                            }
                            break;
                        case '+':
                        case '-':
                            if (ope == '+' || ope == '-' || ope == ',')
                            {
                                continue;
                            }
                            break;
                        default:
                            continue;
                            break;
                    }

                    // プライオリティ確認用
                    ope = expression [ i ];
                    pos = i;
                }
            }
            if (nest != 0)
            {
                throw new Exception("カッコがミスマッチ");
            }
            return pos;
        }

        /// <summary>
        /// 先頭および末尾で対になっているカッコを除去する（式として不要のため）
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        static public string RemoveBrackets(string expression)
        {
            if (expression.First() == '(' && expression.Last() == ')')
            {
                return RemoveBrackets(expression.Substring(1, expression.Length - 2).Trim());
            }
            return expression;
        }

        /// <summary>
        /// 関数型判定
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        static public bool IsFunction(string expression)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(expression.Trim(), @"^[a-zA-Z]+\(.+\)$");
        }


         /// <summary>
        /// 評価
        /// </summary>
        /// <param name="node"></param>
        /// <param name="row">親テーブルの一行</param>
        /// <param name="tbl">テーブル</param>
        /// <returns></returns>
        static public int Compute(Node node, Dictionary<string, int> row = null, Dictionary<string, int> [] tbl = null)
        {
            var ans = 0;

            // 無効ノード
            if (node.Expression == null || node.Type == NodeType.None)
            {
                return ans;
            }

            #region 特殊ケース
            // 関数処理
            if (node.Type == NodeType.Function)
            {
                return Node.sum(node.Left, tbl);
            }

            // 行中のカラム指定
            if (node.Type == NodeType.Item)
            {
                return row[node.Expression];
            }           
            #endregion


            // 左要素の評価
            if (node.Left != null)
            {
                ans = Compute(node.Left, row);
            }

            // 演算子に応じて評価
            if (node.Expression == "-")
            {
                ans -= Compute(node.Right, row);
            }
            else if (node.Expression == "+")
            {
                ans += Compute(node.Right, row);
            }
            else if (node.Expression == "/")
            {
                if (node.Right == null)
                {
                    throw new Exception("乗算ですが項が不足しています");
                }
                ans /= Compute(node.Right, row);
            }
            else if (node.Expression == "*")
            {
                if (node.Right == null)
                {
                    throw new Exception("乗算ですが項が不足しています");
                }
                ans *= Compute(node.Right, row);
            }
            else
            {
                ans = int.Parse(node.Expression);
            }
            return ans;
        }

        /// <summary>
        /// 集合関数＞累計
        /// </summary>
        /// <param name="item"></param>
        /// <param name="dic"></param>
        /// <returns></returns>
        static public int sum(Node left, Dictionary<string, int>[] tbl)
        {
            return (from n in tbl select Node.Compute(left, n)).Sum();
        }
    }
}
