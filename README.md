Sample implementation of [operator-precedence expression parser](https://en.wikipedia.org/wiki/Operator-precedence_parser). The following lexemes and operations are supported:

- **Numbers**:               One or more '0'-'9' characters.Limited  to 32 bit integer values.
- **Variables**:             One or more 'a'-'z' and '_' characters.
- **Grouping**:              ((x + y) * z)
- **Postfix**:               x++, x--
- **Prefix**:                -x, --x, ++x
- **Multiplicative**:        x * y, x / y
- **Additive**:              x + y, x - y
- **Relational**:            x > y, x < y, x >= y, x <= y
- **Equality**:              x == y, x != y
- **Conditional**:           Ternary ?: operator. Expression that evaluates to a positive integer will be considered a true condition.
- **Assignment**:            x = y, x += y, x -= y, x *= y, x /= y
