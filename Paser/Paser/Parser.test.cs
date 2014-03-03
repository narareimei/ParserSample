using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using NUnit.Framework;

namespace ExpressionSample
{
    [TestFixture]
    public partial class Node
    {
        #region 演算子検索のテスト
        [Test]
        [TestCase("2+3", Result=1)]
        [TestCase("20+3", Result=2)]
        [TestCase("200+3", Result=3)]
        [TestCase("2-3", Result=1)]
        [TestCase("20-3", Result=2)]
        [TestCase("200-3", Result=3)]
        [TestCase("abc-x", Result=3)]
        public int pos_２項_加算減算(string expr)
        {
            return Node.GetOperatorPosition(expr);
        }

        [Test]
        [TestCase("2*3", Result=1)]
        [TestCase("20*3", Result=2)]
        [TestCase("200*3", Result=3)]
        [TestCase("2/3", Result=1)]
        [TestCase("20/3", Result=2)]
        [TestCase("200/3", Result=3)]
        public int pos_２項_乗算除算(string expr)
        {
            return Node.GetOperatorPosition(expr);
        }

        [Test]
        [TestCase("2+3*4", Result=1)]
        [TestCase("2*3*4", Result=1)]
        [TestCase("2*3+4", Result=3)]
        public int pos_３項_乗算除算_加算減算(string expr)
        {
            return Node.GetOperatorPosition(expr);
        }

        [Test]
        [TestCase("2+3,4", Result=3)]
        public int pos_３項_カンマ(string expr)
        {
            return Node.GetOperatorPosition(expr);
        }

        [Test]
        [TestCase("(3+2)+4", Result=5)]
        [TestCase("(3+2)", Result=-1)]
        [TestCase("((3+2))", Result=-1)]
        [TestCase("((3+2))+4", Result=7)]
        [TestCase("((3+2)+1)+4", Result=9)]
        [TestCase("Sum(3+2)+4", Result=8)]
        [TestCase("2+Sum(3+2)+4", Result=1)]
        public int pos_かっこ(string expr)
        {
            return Node.GetOperatorPosition(expr);
        }

        [Test]
        [TestCase("((3+2)+1)+4)", Result=9)]
        [TestCase("(((3+2)+1)+4", Result=9)]
        [TestCase("2+Sum((3+2)+4", Result=1)]
        [TestCase("2+Sum(3+2))+4", Result=1)]
        [ExpectedException(typeof(Exception))]
        public int pos_かっこ_例外(string expr)
        {
            return Node.GetOperatorPosition(expr);
        }        
        #endregion

        #region 式解析のテスト
        [Test]
        public void parse_１項_1()
        {
            var node = new Node("2");

            node.Parse();
            Assert.True(node.Expression == "2");
            Assert.True(node.Type       == NodeType.Constant);
            return;
        }
        [Test]
        public void parse_１項_2()
        {
            var node = new Node("+2");

            node.Parse();
            Assert.True(node.Expression         == "+");
            Assert.True(node.Type               == NodeType.Operator);
            Assert.True(node.Right.Expression   == "2");
            return;
        }
        [Test]
        public void parse_１項_3()
        {
            var node = new Node("-2");

            node.Parse();
            Assert.True(node.Expression == "-");
            Assert.True(node.Right.Expression == "2");
            return;
        }
        [Test]
        public void parse_２項()
        {
            var node = new Node("2+3");

            node.Parse();
            Assert.True(node.Left.Expression    == "2");
            Assert.True(node.Expression         == "+");
            Assert.True(node.Type               == NodeType.Operator);
            Assert.True(node.Right.Expression   == "3");
            return;
        }
        [Test]
        public void parse_３項()
        {
            var node = new Node("2+3+4");

            node.Parse();
            Assert.True(node.Left.Expression  == "2");
            Assert.True(node.Expression       == "+");

            Assert.True(node.Right.Left.Expression  == "3");
            Assert.True(node.Right.Expression       == "+");
            Assert.True(node.Right.Right.Expression == "4");
            return;
        }
        [Test]
        public void parse_３項カッコあり()
        {
            var node = new Node("(2+3+4)");

            node.Parse();
            Assert.True(node.Left.Expression  == "2");
            Assert.True(node.Expression       == "+");

            Assert.True(node.Right.Left.Expression  == "3");
            Assert.True(node.Right.Expression       == "+");
            Assert.True(node.Right.Right.Expression == "4");
            return;
        }
        [Test]
        public void parse_4項カッコあり()
        {
            var node = new Node("(2+3+4)+5");

            node.Parse();
            {
                Assert.True(node.Left.Left.Expression  == "2");
                Assert.True(node.Left.Expression       == "+");
                {
                    Assert.True(node.Left.Right.Left.Expression  == "3");
                    Assert.True(node.Left.Right.Expression       == "+");
                    Assert.True(node.Left.Right.Right.Expression == "4");
                }

            }
            Assert.True(node.Expression             == "+");
            Assert.True(node.Right.Expression       == "5");
            return;
        }

        [Test]
        public void parse_2項変数()
        {
            var node = new Node("2+abc");

            node.Parse();

            Assert.True(node.Left.Expression    == "2");
            Assert.True(node.Left.Type == NodeType.Constant);

            Assert.True(node.Expression         == "+");
            Assert.True(node.Type               == NodeType.Operator);

            Assert.True(node.Right.Expression   == "abc");
            Assert.True(node.Right.Type         == NodeType.Item);
            return;
        }
        [Test]
        public void parse_関数_１項_1()
        {
            var node = new Node("SUM(2)");

            node.Parse();
            {
                Assert.True(node.Left.Expression       == "2");

            }
            Assert.True(node.Expression             == "SUM");
            Assert.True(node.Right                  == null);

            return;
        }
        [Test]
        public void parse_関数_１項_2()
        {
            var node = new Node("SUM(-2)");

            node.Parse();
            Assert.True(node.Expression     == "SUM");
            Assert.True(node.Type           == NodeType.Function);
            Assert.True(node.Left.Expression    == "-");
            {
                Assert.True(node.Left.Left              == null);
                Assert.True(node.Left.Right.Expression  == "2");
            }
            Assert.True(node.Right              == null);
        }

        [Test]
        public void parse_関数_１項_3()
        {
            var node = new Node("SUM(2+3)");

            node.Parse();
            {
                Assert.True(node.Left.Left.Expression  == "2");
                Assert.True(node.Left.Expression       == "+");
                Assert.True(node.Left.Right.Expression == "3");

            }
            Assert.True(node.Expression     == "SUM");
            Assert.True(node.Type           == NodeType.Function);
            Assert.True(node.Right          == null);
            return;
        }

        [Test]
        public void parse_関数_２項()
        {
            var node = new Node("SUM(2,3)");

            node.Parse();
            Assert.True(node.Expression     == "SUM");
            Assert.True(node.Type           == NodeType.Function);
            Assert.True(node.Left.Expression  == "2");
            Assert.True(node.Right.Expression == "3");
        }
        [Test]
        public void parse_関数_３項()
        {
            var node = new Node("SUM(2,3+4,5)");

            node.Parse();
            Assert.True(node.Expression         == "SUM");
            Assert.True(node.Type               == NodeType.Function);
            Assert.True(node.Left.Expression    == "2");
            Assert.True(node.Right.Expression  == ",");
            {
                Assert.True(node.Right.Left.Expression  == "+");
                {
                    Assert.True(node.Right.Left.Left.Expression  == "3");
                    Assert.True(node.Right.Left.Right.Expression == "4");
                }
                Assert.True(node.Right.Right.Expression == "5");
            }

            var nodes = Node.GetArgumentNodes(node);
        }
        #endregion

        #region 計算
        [Test]
        [TestCase("1", Result = 1)]
        [TestCase("2", Result = 2)]
        [TestCase("-2", Result = -2)]
        [TestCase("1-2", Result = -1)]
        [TestCase("2*3", Result = 6)]
        [TestCase("6/3", Result = 2)]
        [TestCase("6/3+1", Result = 3)]
        [TestCase("2*(2+3)", Result = 10)]
        [TestCase("2*(2+3)-(5-3)*2", Result = 6)]
        public int 計算(string expression)
        {
            var node = new Node(expression);

            node.Parse();
            //var ans = node.Compute();
            var ans = Node.Compute(node);
            return ans;
        }

        [Test]
        [TestCase("A+B", Result = 3)]
        public int 計算_カラム(string expression)
        {

            var row = new Dictionary<string, int>() { { "A", 1 }, { "B", 2 } };
            var tbl = new[]
                {
                    new Dictionary<string, int>() {{"A", 1}},
                    new Dictionary<string, int>() {{"A", 2}},
                    new Dictionary<string, int>() {{"A", 3}},
                };

            var node = new Node(expression);
            node.Parse();
            var ans = Node.Compute(node, row, tbl);
            return ans;
        }

        [Test]
        [TestCase("SUM(A)", Result = 6)]
        [TestCase("SUM(A+B)", Result = 22)]
        public int 計算_SUM(string expression)
        {
            var row = new Dictionary<string, int>() { { "X", 1 }, { "Y", 2 } };
            var tbl = new[]
                {
                    new Dictionary<string, int>() {{"A", 1},{"B", 3}},
                    new Dictionary<string, int>() {{"A", 2},{"B", 6}},
                    new Dictionary<string, int>() {{"A", 3},{"B", 7}},
                };

            var node = new Node(expression);
            node.Parse();
            var ans = Node.Compute(node, row, tbl);
            return ans;
        }        
        #endregion

        #region 条件関数
        [Test]
        [TestCase("lt(1,2)", Result = 1)]
        [TestCase("lt(1,1)", Result = 0)]
        [TestCase("lt(2,1)", Result = 0)]
        public int 条件_less(string expression)
        {
            var node = new Node(expression);
            node.Parse();
            return Node.Compute(node);
        }
        #endregion

        #region 関数引数取得
        [Test]
        public void parse_関数引数取得1()
        {
            var node = new Node("SUM(2,3+4,5)");

            node.Parse();
            var nodes = Node.GetArgumentNodes(node);

            Assert.True(nodes [ 0 ].Type        == NodeType.Constant);
            Assert.True(nodes [ 0 ].Expression  == "2");
            Assert.True(nodes [ 1 ].Type        == NodeType.Operator);
            Assert.True(nodes [ 1 ].Expression  == "+");
            {
                Assert.True(nodes [ 1 ].Left.Expression     == "3");
                Assert.True(nodes [ 1 ].Right.Expression    == "4");
            }
            Assert.True(nodes [ 2 ].Type        == NodeType.Constant);
            Assert.True(nodes [ 2 ].Expression  == "5");
        }
        [Test]
        public void parse_関数引数取得2()
        {
            var node = new Node("SUM(2,3+4,-5)");

            node.Parse();
            var nodes = Node.GetArgumentNodes(node);

            Assert.True(nodes [ 0 ].Type        == NodeType.Constant);
            Assert.True(nodes [ 0 ].Expression  == "2");
            Assert.True(nodes [ 1 ].Type        == NodeType.Operator);
            Assert.True(nodes [ 1 ].Expression  == "+");
            {
                Assert.True(nodes [ 1 ].Left.Expression     == "3");
                Assert.True(nodes [ 1 ].Right.Expression    == "4");
            }
            Assert.True(nodes [ 2 ].Type        == NodeType.Operator);
            Assert.True(nodes [ 2 ].Expression  == "-");
            {
                Assert.True(nodes [ 2 ].Left                == null);
                Assert.True(nodes [ 2 ].Right.Expression    == "5");
            }
        }
        #endregion

        #region 計算_条件
        [Test]
        [TestCase("if(lt(1,2),10,100)", Result = 10)]
        [TestCase("if(gt(1,2),10,100)", Result = 100)]
        [TestCase("if(eq(1,2),10,100)", Result = 100)]
        [TestCase("if(eq(2,2),10,100)", Result = 10)]
        [TestCase("if(eq(1+1,2*1),5,100)", Result = 5)]
        [TestCase("if(eq(1+1+1,2*1),5,100)", Result = 100)]
        [TestCase("if(lt(X,Y),X*1000,Y*10)", Result = 20)]

        [TestCase("if(and(lt(1,2),gt(2,1)), 50, 999)", Result = 50)]
        [TestCase("if(and(lt(1,2),gt(1,1)), 50, 999)", Result = 999)]
        [TestCase("if(or(lt(1,2),gt(1,1)) , 50, 999)", Result = 50)]
        [TestCase("if(or(lt(3,3),gt(1,1)) , 50, 999)", Result = 999)]

        public int 計算_if(string expression)
        {
            var row = new Dictionary<string, int>() { { "X", 4 }, { "Y", 2 } };
            var tbl = new[]
                {
                    new Dictionary<string, int>() {{"A", 1},{"B", 3}},
                    new Dictionary<string, int>() {{"A", 2},{"B", 6}},
                    new Dictionary<string, int>() {{"A", 3},{"B", 7}},
                };

            var node = new Node(expression);

            node.Parse();
            return Node.Compute(node, row, tbl);
        }        
        #endregion
    }
}

