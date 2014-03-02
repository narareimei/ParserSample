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

        // NUnit用
        public Node()
        {
        }

        public Node(string expression)
        {
            this.Expression = RemoveBrackets(expression.Trim());
        }

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

        static public bool IsFunction(string expression)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(expression.Trim(), @"^[a-zA-Z]+\(.+\)$");
        }

        public int Compute()
        {
            var ans = 0;

            if (this.Expression == null)
            {
                return ans;
            }

            if (this.Left != null)
            {
                ans = this.Left.Compute();
            }

            if (this.Expression == "-")
            {
                ans -= this.Right.Compute();
            }else
            if (this.Expression == "+")
            {
                ans += this.Right.Compute();
            }else
            if (this.Expression == "/")
            {
                if (this.Right == null)
                {
                    throw new Exception("乗算ですが項が不足しています");
                }
                ans /= this.Right.Compute();
            }
            else
            if (this.Expression == "*")
            {
                if (this.Right == null)
                {
                    throw new Exception("乗算ですが項が不足しています");
                }
                ans *= this.Right.Compute();
            }else
            {
                ans = int.Parse(this.Expression);
            }

            return ans;
        }
    }
}
