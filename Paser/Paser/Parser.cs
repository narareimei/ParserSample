using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using NUnit.Framework;

namespace ExpressionSample
{
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
        public List<Node>  Nodes = new List<Node>();
        public NodeType Type = NodeType.None;
        // 関数名辞書
        static readonly private Dictionary<string, string> functionDictionary;
        static readonly private string functionMatcher;

        // 静的コンストラクタ
        static Node()
        {
            functionDictionary = new Dictionary<string, string>()
            {
                {"SUM",""},{"AVG",""},{"lt",""},{"eq",""},{"gt",""},{"if",""},{"and",""},{"or",""},
            };

            var matcher = "";
            foreach (var key in functionDictionary.Keys)
            {
                matcher += ( key + "|" );
            }
            if (matcher.Last() == '|')
            {
                matcher = matcher.Remove(matcher.Length-1);
            }
            functionMatcher = matcher;
        }


        // NUnit用コンストラクタ
        public Node()
        {
        }

        // コンストラクタ
        public Node(string expression)
        {
            this.Expression = RemoveBrackets(expression.Trim());
        }

        /// <summary>
        /// 式解析
        /// </summary>
        /// <remarks>再帰的に呼び出される</remarks>
        public void Parse()
        {
            int pos = GetOperatorPosition((this.Expression));

            // 符号として+-が指定されるケース
            if (pos == 0)
            {
                // TODO いきなり+-以外の演算子が出現したら異常
                this.Left       = null;
                this.Right      = new Node(this.Expression.Substring(pos + 1));
                this.Right.Parse();
                this.Expression = this.Expression[pos].ToString();
                this.Type       = NodeType.Operator;
            }
            // 「項 演算子 項」のケース
            else if (pos > 0)
            {
                this.Left       = new Node(this.Expression.Substring(0, pos));
                this.Left.Parse();
                this.Right      = new Node(this.Expression.Substring(pos+1));
                this.Right.Parse();
                this.Expression = this.Expression [ pos ].ToString();
                this.Type       = NodeType.Operator;
            }
            // 関数「Func(項・・・）」のケース
            else if (IsFunction(this.Expression) == true)
            {
                // 関数名だけをExpressionとして保存する。本当は属性を何か持ちたい
                var functionName = ( new Regex(@"^"+functionMatcher) ).Match(this.Expression).ToString();
                if (functionDictionary.ContainsKey(functionName) == false)
                {
                    throw new Exception("サポートされていない関数名が指定されています（"+functionName+"）");
                }
                else
                {
                    // 一旦オペランド部を解析する
                    var operandNode = new Node(Regex.Replace(Expression, @"^(" + functionMatcher +@")\((.+)\)$", "$2"));
                    operandNode.Parse();

                    // 多項の引数の場合
                    if (operandNode.Expression == ",")
                    {
                        this.Left  = operandNode.Left;
                        this.Right = operandNode.Right;
                    }
                    // １項の引数の場合
                    else
                    {
                        this.Left = operandNode;
                        this.Right = null;
                    }
                    this.Expression = functionName;
                    this.Type       = NodeType.Function;
                }
            }
            // 式を含まないケース
            else
            {
                if((new Regex(@".*[^0-9].*")).IsMatch(this.Expression) )
                {
                    this.Type   = NodeType.Item;
                }else{
                    this.Type   = NodeType.Constant;
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
                                //throw new Exception("カンマが２度記述されている（３項以上となっている）");
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
                if (node.Expression == "SUM")
                {
                    return Node.sum(node.Left, tbl);
                }
                else
                if (node.Expression == "lt")
                {
                    return Node.lt(node.Left, node.Right, row, tbl);
                }
                else
                if (node.Expression == "eq")
                {
                    return Node.eq(node.Left, node.Right, row, tbl);
                }
                else
                if (node.Expression == "gt")
                {
                    return Node.gt(node.Left, node.Right, row, tbl);
                }
                else
                if (node.Expression == "if")
                {
                    var nodes = GetArgumentNodes(node);
                    return Node.iif( nodes, row, tbl);
                }
                else
                if (node.Expression == "and")
                {
                    var nodes = GetArgumentNodes(node);
                    return Node.and( nodes, row, tbl);
                }
                else
                if (node.Expression == "or")
                {
                    var nodes = GetArgumentNodes(node);
                    return Node.or( nodes, row, tbl);
                }
                else
                {
                    throw new Exception("この関数は実装されていません（"+node.Expression+"）");
                }
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

        /// <summary>
        /// 比較「＜」
        /// </summary>
        /// <param name="left"></param>
        /// <param name="tbl"></param>
        /// <returns></returns>
        static public int lt(Node left, Node right, Dictionary<string, int> row = null, Dictionary<string, int> [ ] tbl = null)
        {
            var leftVal  = Node.Compute(left,  row, tbl);
            var rightVal = Node.Compute(right, row, tbl);

            if (leftVal < rightVal)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
        /// <summary>
        /// 比較「＝」
        /// </summary>
        /// <param name="left"></param>
        /// <param name="tbl"></param>
        /// <returns></returns>
        static public int eq(Node left, Node right, Dictionary<string, int> row = null, Dictionary<string, int>[] tbl = null)
        {
            var leftVal = Node.Compute(left, row, tbl);
            var rightVal = Node.Compute(right, row, tbl);

            if (leftVal == rightVal)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
        /// <summary>
        /// 比較「＞」
        /// </summary>
        /// <param name="left"></param>
        /// <param name="tbl"></param>
        /// <returns></returns>
        static public int gt(Node left, Node right, Dictionary<string, int> row = null, Dictionary<string, int>[] tbl = null)
        {
            var leftVal = Node.Compute(left, row, tbl);
            var rightVal = Node.Compute(right, row, tbl);

            if (leftVal > rightVal)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// IF
        /// </summary>
        /// <param name="left"></param>
        /// <param name="tbl"></param>
        /// <returns></returns>
        static public int iif(Node [] nodes, Dictionary<string, int> row = null, Dictionary<string, int>[] tbl = null)
        {
            var condition = Node.Compute(nodes[0], row, tbl);
            if (condition == 1)
            {
                return Node.Compute(nodes[1], row, tbl);
            }
            else
            {
                return Node.Compute(nodes[2], row, tbl);
            }
        }

        /// <summary>
        /// AND
        /// </summary>
        /// <param name="left"></param>
        /// <param name="tbl"></param>
        /// <returns></returns>
        static public int and(Node[] nodes, Dictionary<string, int> row = null, Dictionary<string, int>[] tbl = null)
        {
            foreach (var node in nodes)
            {
                if (Node.Compute(node, row, tbl) == 0)
                    return 0;
            }
            return 1;
        }

        /// <summary>
        /// OR
        /// </summary>
        /// <param name="left"></param>
        /// <param name="tbl"></param>
        /// <returns></returns>
        static public int or(Node[] nodes, Dictionary<string, int> row = null, Dictionary<string, int>[] tbl = null)
        {
            foreach (var node in nodes)
            {
                if (Node.Compute(node, row, tbl) == 1)
                    return 1;
            }
            return 0;
        }

        /// <summary>
        /// ツリー形式になっている関数の引数ノードを配列として返す
        /// </summary>
        /// <param name="root"></param>
        /// <returns></returns>
        static public Node [ ] GetArgumentNodes(Node root)
        {
            var nodes = new List<Node>();

            var node = root;
            while(true)
            {
                // ２回目以降（子要素）が関数型の場合もそれを最後として登録する
                if (node.Type == NodeType.Constant || node.Type == NodeType.Item ||
                    ( node.Type == NodeType.Operator && node.Expression != ",") ||
                    ( node.Type == NodeType.Function && node != root)
                    )
                {
                    nodes.Add(node);
                    break;
                }
                if (node.Left != null)
                {
                    nodes.Add(node.Left);
                }
                node = node.Right;
            }
            return nodes.ToArray();
        }

    }
}
