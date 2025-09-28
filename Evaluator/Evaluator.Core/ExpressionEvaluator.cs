using System.Globalization;

namespace Evaluator.Core
{
    public class ExpressionEvaluator
    {
        public static double Evaluate(string infix)
        {
            var tokens = Tokenize(infix);               // 🔹 Nuevo: separar en tokens
            var postfix = InfixToPostfix(tokens);       // 🔹 Adaptado: usa tokens, no chars
            return Calculate(postfix);                  // 🔹 Adaptado: evalúa tokens decimales
        }

        // ===============================================================
        // 🔸 NUEVO MÉTODO: Tokenización (permite números multi-dígito y decimales)
        // ===============================================================
        private static List<string> Tokenize(string infix)
        {
            var tokens = new List<string>();
            var current = string.Empty;

            foreach (char c in infix)
            {
                if (char.IsWhiteSpace(c)) continue; // ignorar espacios

                // 🔹 Si el caracter es dígito o punto, se acumula en el token actual
                if (char.IsDigit(c) || c == '.')
                {
                    current += c;
                }
                else if (IsOperator(c))
                {
                    // 🔹 Si ya se venía formando un número, se agrega antes de meter el operador
                    if (!string.IsNullOrEmpty(current))
                    {
                        tokens.Add(current);
                        current = string.Empty;
                    }
                    tokens.Add(c.ToString()); // operador como token
                }
                else
                {
                    throw new Exception($"Carácter inválido en la expresión: {c}");
                }
            }

            // 🔹 Si al final queda un número pendiente, se agrega
            if (!string.IsNullOrEmpty(current))
                tokens.Add(current);

            return tokens;
        }

        // ===============================================================
        // 🔸 Infix → Postfix (adaptado a tokens)
        // ===============================================================
        private static string InfixToPostfix(List<string> tokens)
        {
            var stack = new Stack<string>();
            var postfix = new List<string>();

            foreach (var token in tokens)
            {
                if (IsNumber(token))
                {
                    postfix.Add(token);
                }
                else if (IsOperator(token[0]))
                {
                    if (token == ")")
                    {
                        while (stack.Peek() != "(")
                            postfix.Add(stack.Pop());
                        stack.Pop(); // eliminar "("
                    }
                    else
                    {
                        if (stack.Count > 0)
                        {
                            if (PriorityInfix(token[0]) > PriorityStack(stack.Peek()[0]))
                            {
                                stack.Push(token);
                            }
                            else
                            {
                                postfix.Add(stack.Pop());
                                stack.Push(token);
                            }
                        }
                        else
                        {
                            stack.Push(token);
                        }
                    }
                }
            }

            while (stack.Count > 0)
                postfix.Add(stack.Pop());

            return string.Join(" ", postfix); // 🔹 se separan los tokens por espacios
        }

        private static bool IsOperator(char item)
            => item is '^' or '/' or '*' or '%' or '+' or '-' or '(' or ')';

        private static int PriorityInfix(char op) => op switch
        {
            '^' => 4,
            '*' or '/' or '%' => 2,
            '-' or '+' => 1,
            '(' => 5,
            _ => throw new Exception("Operador inválido."),
        };

        private static int PriorityStack(char op) => op switch
        {
            '^' => 3,
            '*' or '/' or '%' => 2,
            '-' or '+' => 1,
            '(' => 0,
            _ => throw new Exception("Operador inválido."),
        };

        private static bool IsNumber(string token)
        {
            return double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out _);
        }

        // ===============================================================
        // 🔸 Cálculo de expresión Postfija (adaptado a tokens decimales)
        // ===============================================================
        private static double Calculate(string postfix)
        {
            var stack = new Stack<double>();
            var tokens = postfix.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            foreach (var token in tokens)
            {
                if (IsOperator(token[0]) && token.Length == 1)
                {
                    var op2 = stack.Pop();
                    var op1 = stack.Pop();
                    stack.Push(Calculate(op1, token[0], op2));
                }
                else
                {
                    // 🔹 Ahora soporta decimales, no solo dígitos simples
                    stack.Push(double.Parse(token, CultureInfo.InvariantCulture));
                }
            }

            return stack.Peek();
        }

        private static double Calculate(double op1, char item, double op2) => item switch
        {
            '*' => op1 * op2,
            '/' => op1 / op2,
            '^' => Math.Pow(op1, op2),
            '+' => op1 + op2,
            '-' => op1 - op2,
            _ => throw new Exception("Operador inválido."),
        };
    }
}