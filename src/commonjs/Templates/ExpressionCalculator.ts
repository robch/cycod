/**
 * Exception thrown during expression calculation
 */
export class CalcException extends Error {
    public readonly expression: string;
    public readonly position: number;

    constructor(message: string, expression: string, position: number) {
        super(message);
        this.name = 'CalcException';
        this.expression = expression;
        this.position = position;
    }
}

/**
 * Mathematical expression calculator with support for variables, functions, and operators
 */
export class ExpressionCalculator {
    /**
     * Token types for expression parsing
     */
    public enum TokenType {
        Identifier = 'Identifier',
        Number = 'Number',
        String = 'String',
        Bool = 'Bool',
        Equal = 'Equal',            // =
        EqualEqual = 'EqualEqual',       // ==
        NotEqual = 'NotEqual',         // !=
        LessThan = 'LessThan',         // <
        LessThanEqual = 'LessThanEqual',    // <=
        GreaterThan = 'GreaterThan',      // >
        GreaterThanEqual = 'GreaterThanEqual', // >=
        Plus = 'Plus',
        Minus = 'Minus',
        Times = 'Times',
        Divide = 'Divide',
        Mod = 'Mod',
        Div = 'Div',
        LogicalAnd = 'LogicalAnd',
        LogicalOr = 'LogicalOr',
        LogicalNot = 'LogicalNot',
        BitwiseAnd = 'BitwiseAnd',
        BitwiseOr = 'BitwiseOr',
        BitwiseNot = 'BitwiseNot',
        Power = 'Power',
        OpenParen = 'OpenParen',
        CloseParen = 'CloseParen',
        Comma = 'Comma',
        Eos = 'Eos'
    }

    /**
     * Numeric radix enumeration
     */
    public enum Radix {
        Bin = 2,
        Oct = 8,
        Dec = 10,
        Hex = 16
    }

    private readonly _functions: ExpressionCalculator.Function[] = [];
    private readonly _constants: ExpressionCalculator.Constant[] = [];
    private readonly _variables: ExpressionCalculator.Variable[] = [];

    private readonly _constantE: ExpressionCalculator.Constant;
    private readonly _constantPI: ExpressionCalculator.Constant;


    private _expression: string = '';
    private _position: number = 0;
    private _nextPosition: number = 0;
    private _tokenType: ExpressionCalculator.ExpressionCalculator.TokenType = ExpressionCalculator.ExpressionCalculator.TokenType.Eos;
    private _token: string = '';

    constructor() {
        this._constantE = new ExpressionCalculator.Constant('E', 2.71828182845905);
        this._constantPI = new ExpressionCalculator.Constant('PI', 3.14159265358979);
        this.addConstant(this._constantE);
        this.addConstant(this._constantPI);

        // Math functions
        this.addFunction(new ExpressionCalculator.Function('ABS', () => this.abs()));
        this.addFunction(new ExpressionCalculator.Function('ACOS', () => this.acos()));
        this.addFunction(new ExpressionCalculator.Function('ASIN', () => this.asin()));
        this.addFunction(new ExpressionCalculator.Function('ATAN', () => this.atan()));
        this.addFunction(new ExpressionCalculator.Function('ATAN2', () => this.atan2()));
        this.addFunction(new ExpressionCalculator.Function('CEIL', () => this.ceil()));
        this.addFunction(new ExpressionCalculator.Function('COS', () => this.cos()));
        this.addFunction(new ExpressionCalculator.Function('COSH', () => this.cosh()));
        this.addFunction(new ExpressionCalculator.Function('EXP', () => this.exp()));
        this.addFunction(new ExpressionCalculator.Function('FLOOR', () => this.floor()));
        this.addFunction(new ExpressionCalculator.Function('LOG', () => this.log()));
        this.addFunction(new ExpressionCalculator.Function('LOG10', () => this.log10()));
        this.addFunction(new ExpressionCalculator.Function('SIN', () => this.sin()));
        this.addFunction(new ExpressionCalculator.Function('SINH', () => this.sinh()));
        this.addFunction(new ExpressionCalculator.Function('SQRT', () => this.sqrt()));
        this.addFunction(new ExpressionCalculator.Function('TAN', () => this.tan()));
        this.addFunction(new ExpressionCalculator.Function('TANH', () => this.tanh()));
        this.addFunction(new ExpressionCalculator.Function('TRUNCATE', () => this.truncate()));
        this.addFunction(new ExpressionCalculator.Function('MAX', () => this.max()));
        this.addFunction(new ExpressionCalculator.Function('MIN', () => this.min()));

        // String functions
        this.addFunction(new ExpressionCalculator.Function('TOLOWER', () => this.tolower()));
        this.addFunction(new ExpressionCalculator.Function('TOUPPER', () => this.toupper()));
        this.addFunction(new ExpressionCalculator.Function('EQUALS', () => this.equals()));
        this.addFunction(new ExpressionCalculator.Function('CONTAINS', () => this.contains()));
        this.addFunction(new ExpressionCalculator.Function('STARTSWITH', () => this.startsWith()));
        this.addFunction(new ExpressionCalculator.Function('ENDSWITH', () => this.endsWith()));
        this.addFunction(new ExpressionCalculator.Function('ISEMPTY', () => this.isEmpty()));
    }

    /**
     * Evaluates a mathematical/logical expression
     * @param str The expression string to evaluate
     * @returns The result of the evaluation
     */
    public evaluate(str: string): any {
        this._expression = str;
        this._position = this._nextPosition = 0;

        this.skipWhiteSpace();
        this.nextToken();

        const value = this.statement();
        if (this._tokenType !== ExpressionCalculator.ExpressionCalculator.TokenType.Eos) {
            throw this.unexpectedCharacterCalcException(this._position);
        }

        return value;
    }

    /**
     * Adds a function to the calculator
     * @param func The function to add
     */
    public addFunction(func: ExpressionCalculator.Function): void {
        func.name = func.name.toUpperCase();
        this._functions.push(func);
    }

    /**
     * Adds a constant to the calculator
     * @param constant The constant to add
     */
    public addConstant(constant: ExpressionCalculator.Constant): void {
        constant.name = constant.name.toUpperCase();
        this._constants.push(constant);
    }

    /**
     * Adds a variable to the calculator
     * @param variable The variable to add
     */
    public addVariable(variable: ExpressionCalculator.Variable): void {
        variable.name = variable.name.toUpperCase();
        this._variables.push(variable);
    }

    private statement(): any {
        let isAssignment = false;
        let variableName: string | null = null;

        if (this._tokenType === ExpressionCalculator.ExpressionCalculator.TokenType.Identifier && 
            this.functionFromString(this._token) === null && 
            this.constantFromString(this._token) === null) {
            variableName = this._token;
            this.nextToken();
            if (this._tokenType === ExpressionCalculator.ExpressionCalculator.TokenType.Equal) {
                this.nextToken();
                isAssignment = true;
            }
        }

        if (!isAssignment) {
            this._position = this._nextPosition = 0;
            this.skipWhiteSpace();
            this.nextToken();
        }

        const value = this.bitExpression();
        if (isAssignment) {
            let variable = this.variableFromString(variableName!);
            if (variable === null) {
                variable = new ExpressionCalculator.Variable(variableName!, value);
                this.addVariable(variable);
            }
            variable.value = value;
        }
        return value;
    }

    private bitExpression(): any {
        let value = this.bitTerm();
        while (true) {
            if (this._tokenType === ExpressionCalculator.TokenType.LogicalOr) {
                this.nextToken();
                const value2 = this.bitTerm();
                if (typeof value !== 'boolean' || typeof value2 !== 'boolean') {
                    throw new CalcException('Expected boolean', this._expression, this._position);
                }
                value = value || value2;
            } else if (this._tokenType === ExpressionCalculator.TokenType.BitwiseOr) {
                this.nextToken();
                const value2 = this.bitTerm();
                if (value >= Number.MIN_SAFE_INTEGER && value <= Number.MAX_SAFE_INTEGER) {
                    const l1 = Math.floor(value);
                    if (value2 >= Number.MIN_SAFE_INTEGER && value2 <= Number.MAX_SAFE_INTEGER) {
                        const l2 = Math.floor(value2);
                        value = l1 | l2;
                    } else {
                        throw new CalcException('Result to large. Bitwise operation conversion impossible.', this._expression, this._position);
                    }
                } else {
                    throw new CalcException('Result to large. Bitwise operation impossible.', this._expression, this._position);
                }
            } else {
                break;
            }
        }
        return value;
    }

    private comparisonExpression(): any {
        const value = this.bitFactor();

        if (this._tokenType === ExpressionCalculator.TokenType.EqualEqual) {
            this.nextToken();
            const value2 = this.bitFactor();

            if (typeof value === 'string' && typeof value2 === 'string') {
                return value === value2;
            } else if (typeof value === 'boolean' && typeof value2 === 'boolean') {
                return value === value2;
            } else if (typeof value === 'number' && typeof value2 === 'number') {
                return Math.abs(value - value2) < 1e-10;
            } else {
                return value.toString() === value2.toString();
            }
        } else if (this._tokenType === ExpressionCalculator.TokenType.NotEqual) {
            this.nextToken();
            const value2 = this.bitFactor();

            if (typeof value === 'string' && typeof value2 === 'string') {
                return value !== value2;
            } else if (typeof value === 'boolean' && typeof value2 === 'boolean') {
                return value !== value2;
            } else if (typeof value === 'number' && typeof value2 === 'number') {
                return Math.abs(value - value2) >= 1e-10;
            } else {
                return value.toString() !== value2.toString();
            }
        } else if (this._tokenType === ExpressionCalculator.TokenType.LessThan) {
            this.nextToken();
            const value2 = this.bitFactor();

            if (typeof value === 'number' && typeof value2 === 'number') {
                return value < value2;
            } else if (typeof value === 'string' && typeof value2 === 'string') {
                return value < value2;
            } else {
                throw new CalcException('Cannot compare these types with <', this._expression, this._position);
            }
        } else if (this._tokenType === ExpressionCalculator.TokenType.LessThanEqual) {
            this.nextToken();
            const value2 = this.bitFactor();

            if (typeof value === 'number' && typeof value2 === 'number') {
                return value <= value2;
            } else if (typeof value === 'string' && typeof value2 === 'string') {
                return value <= value2;
            } else {
                throw new CalcException('Cannot compare these types with <=', this._expression, this._position);
            }
        } else if (this._tokenType === ExpressionCalculator.TokenType.GreaterThan) {
            this.nextToken();
            const value2 = this.bitFactor();

            if (typeof value === 'number' && typeof value2 === 'number') {
                return value > value2;
            } else if (typeof value === 'string' && typeof value2 === 'string') {
                return value > value2;
            } else {
                throw new CalcException('Cannot compare these types with >', this._expression, this._position);
            }
        } else if (this._tokenType === ExpressionCalculator.TokenType.GreaterThanEqual) {
            this.nextToken();
            const value2 = this.bitFactor();

            if (typeof value === 'number' && typeof value2 === 'number') {
                return value >= value2;
            } else if (typeof value === 'string' && typeof value2 === 'string') {
                return value >= value2;
            } else {
                throw new CalcException('Cannot compare these types with >=', this._expression, this._position);
            }
        }

        return value;
    }

    private bitTerm(): any {
        let value = this.comparisonExpression();
        while (true) {
            if (this._tokenType === ExpressionCalculator.TokenType.LogicalAnd) {
                this.nextToken();
                const value2 = this.comparisonExpression();
                if (typeof value !== 'boolean' || typeof value2 !== 'boolean') {
                    throw new CalcException('Expected boolean', this._expression, this._position);
                }
                value = value && value2;
            } else if (this._tokenType === ExpressionCalculator.TokenType.BitwiseAnd) {
                this.nextToken();
                const value2 = this.comparisonExpression();
                if (value >= Number.MIN_SAFE_INTEGER && value <= Number.MAX_SAFE_INTEGER) {
                    const l1 = Math.floor(value);
                    if (value2 >= Number.MIN_SAFE_INTEGER && value2 <= Number.MAX_SAFE_INTEGER) {
                        const l2 = Math.floor(value2);
                        value = l1 & l2;
                    } else {
                        throw new CalcException('Result to large. Bitwise operation conversion impossible.', this._expression, this._position);
                    }
                } else {
                    throw new CalcException('Result to large. Bitwise operation impossible.', this._expression, this._position);
                }
            } else {
                break;
            }
        }
        return value;
    }

    private bitFactor(): any {
        let value = this.expression();
        while (true) {
            if (this._tokenType === ExpressionCalculator.TokenType.BitwiseAnd) {
                this.nextToken();
                const value2 = this.expression();
                if (value >= Number.MIN_SAFE_INTEGER && value <= Number.MAX_SAFE_INTEGER) {
                    const l1 = Math.floor(value);
                    if (value2 >= Number.MIN_SAFE_INTEGER && value2 <= Number.MAX_SAFE_INTEGER) {
                        const l2 = Math.floor(value2);
                        value = l1 & l2;
                    } else {
                        throw new CalcException('Result to large. Bitwise operation conversion impossible.', this._expression, this._position);
                    }
                } else {
                    throw new CalcException('Result to large. Bitwise operation impossible.', this._expression, this._position);
                }
            } else {
                break;
            }
        }
        return value;
    }

    private expression(): any {
        let value = this.term();
        while (true) {
            if (this._tokenType === ExpressionCalculator.TokenType.Plus) {
                this.nextToken();
                const value2 = this.term();
                const precision = value === 0 || value2 === 0 ? 1 : Math.pow(10, Math.max(Math.ceil(Math.log10(Math.abs(value))), Math.ceil(Math.log10(Math.abs(value2)))) - 15);
                value = this.round(value + value2, precision);
            } else if (this._tokenType === ExpressionCalculator.TokenType.Minus) {
                this.nextToken();
                const value2 = this.term();
                const precision = value === 0 || value2 === 0 ? 1 : Math.pow(10, Math.max(Math.ceil(Math.log10(Math.abs(value))), Math.ceil(Math.log10(Math.abs(value2)))) - 15);
                value = this.round(value - value2, precision);
            } else {
                break;
            }
        }
        return value;
    }

    private term(): any {
        let value = this.unary();
        while (true) {
            if (this._tokenType === ExpressionCalculator.TokenType.Times) {
                this.nextToken();
                value = this.exact(value * this.unary());
            } else if (this._tokenType === ExpressionCalculator.TokenType.Divide) {
                this.nextToken();
                value = this.exact(value / this.unary());
            } else if (this._tokenType === ExpressionCalculator.TokenType.Mod) {
                this.nextToken();
                value = this.exact(value % this.unary());
            } else if (this._tokenType === ExpressionCalculator.TokenType.Div) {
                this.nextToken();
                value = this.exact(Math.floor(value / this.unary()));
            } else {
                break;
            }
        }
        return value;
    }

    private unary(): any {
        let negate = false;
        let bitwiseNot = false;
        let logicalNot = false;
        if (this._tokenType === ExpressionCalculator.TokenType.Minus) {
            this.nextToken();
            negate = true;
        } else if (this._tokenType === ExpressionCalculator.TokenType.Plus) {
            this.nextToken();
            negate = false;
        } else if (this._tokenType === ExpressionCalculator.TokenType.BitwiseNot) {
            this.nextToken();
            bitwiseNot = true;
        } else if (this._tokenType === ExpressionCalculator.TokenType.LogicalNot) {
            this.nextToken();
            logicalNot = true;
        }
        let value = this.exponent();
        if (negate) {
            value = value * -1.0;
        }
        if (logicalNot) {
            if (typeof value !== 'boolean') {
                throw new CalcException('Expected boolean', this._expression, this._position);
            }
            value = !value;
        }
        if (bitwiseNot) {
            if (value >= Number.MIN_SAFE_INTEGER && value <= Number.MAX_SAFE_INTEGER) {
                let l = Math.floor(value);
                l = ~l;
                value = l;
            } else {
                throw new CalcException('Result to large. Bitwise operation impossible.', this._expression, this._position);
            }
        }
        return value;
    }

    private exponent(): any {
        let value = this.factor();
        if (this._tokenType === ExpressionCalculator.TokenType.Power) {
            this.nextToken();
            value = this.exact(Math.pow(value, this.unary()));
        }
        return value;
    }

    private factor(): any {
        let value: any;
        if (this._tokenType === ExpressionCalculator.TokenType.Number) {
            value = parseFloat(this._token);
            this.nextToken();
        } else if (this._tokenType === ExpressionCalculator.TokenType.String) {
            value = this._token;
            this.nextToken();
        } else if (this._tokenType === ExpressionCalculator.TokenType.Bool) {
            value = this._token === 'true';
            this.nextToken();
        } else if (this._tokenType === ExpressionCalculator.TokenType.Identifier) {
            const func = this.functionFromString(this._token);
            const constant = this.constantFromString(this._token);
            const variable = this.variableFromString(this._token);

            if (func !== null) {
                this.nextToken();
                value = func.delegate();
                if (typeof value === 'number') {
                    value = this.exact(value);
                }
            } else if (constant !== null) {
                this.nextToken();
                value = constant.value;
                if (typeof value === 'number') {
                    value = this.exact(value);
                }
            } else if (variable !== null) {
                this.nextToken();
                value = variable.value;
                if (typeof value === 'number') {
                    value = this.exact(value);
                }
            } else {
                throw new CalcException('Undefined symbol', this._expression, this._position);
            }
        } else if (this._tokenType === ExpressionCalculator.TokenType.OpenParen) {
            this.nextToken();
            value = this.bitExpression();
            if (this._tokenType !== ExpressionCalculator.TokenType.CloseParen) {
                throw new CalcException('Expected close parenthesis', this._expression, this._position);
            }
            this.nextToken();
        } else {
            throw new CalcException('Expected number, function, or constant', this._expression, this._position);
        }
        return value;
    }

    private nextToken(): void {
        this._position = this._nextPosition;
        if (this._nextPosition >= this._expression.length) {
            this._tokenType = ExpressionCalculator.ExpressionCalculator.TokenType.Eos;
        } else {
            const char = this._expression[this._nextPosition];
            if (this.isLetter(char) || char === '_') {
                while (this._nextPosition < this._expression.length &&
                      (this.isLetter(this._expression[this._nextPosition]) ||
                       this.isDigit(this._expression[this._nextPosition]) ||
                       this._expression[this._nextPosition] === '_')) {
                    this._nextPosition++;
                }

                this._token = this._expression.substring(this._position, this._nextPosition);
                const upperToken = this._token.toUpperCase();
                switch (upperToken) {
                    case 'MOD':
                        this._tokenType = ExpressionCalculator.TokenType.Mod;
                        break;
                    case 'DIV':
                        this._tokenType = ExpressionCalculator.TokenType.Div;
                        break;
                    case 'TRUE':
                        this._tokenType = ExpressionCalculator.TokenType.Bool;
                        break;
                    case 'FALSE':
                        this._tokenType = ExpressionCalculator.TokenType.Bool;
                        break;
                    default:
                        this._tokenType = ExpressionCalculator.TokenType.Identifier;
                        break;
                }

                this.skipWhiteSpace();
            } else if (this.isDigit(char) || char === '.') {
                while (this._nextPosition < this._expression.length && 
                      (this.isDigit(this._expression[this._nextPosition]) || this._expression[this._nextPosition] === '.')) {
                    this._nextPosition++;
                }

                this._token = this._expression.substring(this._position, this._nextPosition);
                this._tokenType = ExpressionCalculator.TokenType.Number;
                this.skipWhiteSpace();
            } else if (char === '"') {
                this._nextPosition++;
                while (this._nextPosition < this._expression.length && this._expression[this._nextPosition] !== '"') {
                    this._nextPosition++;
                }
                if (this._nextPosition >= this._expression.length) {
                    throw new CalcException('Expected closing quote', this._expression, this._position);
                }

                this._token = this._expression.substring(this._position + 1, this._nextPosition);
                this._nextPosition++;
                this._tokenType = ExpressionCalculator.TokenType.String;
                this.skipWhiteSpace();
            } else if (this._expression.substring(this._nextPosition).startsWith('&&')) {
                this._tokenType = ExpressionCalculator.TokenType.LogicalAnd;
                this._token = '&&';
                this._nextPosition += 2;
                this.skipWhiteSpace();
            } else if (this._expression.substring(this._nextPosition).startsWith('||')) {
                this._tokenType = ExpressionCalculator.TokenType.LogicalOr;
                this._token = '||';
                this._nextPosition += 2;
                this.skipWhiteSpace();
            } else {
                switch (char) {
                    case '=':
                        if (this._nextPosition + 1 < this._expression.length &&
                            this._expression[this._nextPosition + 1] === '=') {
                            this._tokenType = ExpressionCalculator.TokenType.EqualEqual;
                            this._nextPosition++;
                        } else {
                            this._tokenType = ExpressionCalculator.TokenType.Equal;
                        }
                        break;
                    case '!':
                        if (this._nextPosition + 1 < this._expression.length &&
                            this._expression[this._nextPosition + 1] === '=') {
                            this._tokenType = ExpressionCalculator.TokenType.NotEqual;
                            this._nextPosition++;
                        } else {
                            this._tokenType = ExpressionCalculator.TokenType.LogicalNot;
                        }
                        break;
                    case '<':
                        if (this._nextPosition + 1 < this._expression.length &&
                            this._expression[this._nextPosition + 1] === '=') {
                            this._tokenType = ExpressionCalculator.TokenType.LessThanEqual;
                            this._nextPosition++;
                        } else {
                            this._tokenType = ExpressionCalculator.TokenType.LessThan;
                        }
                        break;
                    case '>':
                        if (this._nextPosition + 1 < this._expression.length &&
                            this._expression[this._nextPosition + 1] === '=') {
                            this._tokenType = ExpressionCalculator.TokenType.GreaterThanEqual;
                            this._nextPosition++;
                        } else {
                            this._tokenType = ExpressionCalculator.TokenType.GreaterThan;
                        }
                        break;
                    case '+':
                        this._tokenType = ExpressionCalculator.TokenType.Plus;
                        break;
                    case '-':
                        this._tokenType = ExpressionCalculator.TokenType.Minus;
                        break;
                    case '*':
                        if (this._nextPosition + 1 < this._expression.length &&
                            this._expression[this._nextPosition + 1] === '*') {
                            this._tokenType = ExpressionCalculator.TokenType.Power;
                            this._nextPosition++;
                        } else {
                            this._tokenType = ExpressionCalculator.TokenType.Times;
                        }
                        break;
                    case '/':
                        this._tokenType = ExpressionCalculator.TokenType.Divide;
                        break;
                    case '(':
                        this._tokenType = ExpressionCalculator.TokenType.OpenParen;
                        break;
                    case ')':
                        this._tokenType = ExpressionCalculator.TokenType.CloseParen;
                        break;
                    case ',':
                        this._tokenType = ExpressionCalculator.TokenType.Comma;
                        break;
                    case '~':
                        this._tokenType = ExpressionCalculator.TokenType.BitwiseNot;
                        break;
                    case '&':
                        if (this._nextPosition + 1 < this._expression.length &&
                            this._expression[this._nextPosition + 1] === '&') {
                            this._tokenType = ExpressionCalculator.TokenType.LogicalAnd;
                            this._nextPosition++;
                        } else {
                            this._tokenType = ExpressionCalculator.TokenType.BitwiseAnd;
                        }
                        break;
                    case '^':
                        this._tokenType = ExpressionCalculator.TokenType.Power;
                        break;
                    case '%':
                        this._tokenType = ExpressionCalculator.TokenType.Mod;
                        break;
                    case '|':
                        if (this._nextPosition + 1 < this._expression.length &&
                            this._expression[this._nextPosition + 1] === '|') {
                            this._tokenType = ExpressionCalculator.TokenType.LogicalOr;
                            this._nextPosition++;
                        } else {
                            this._tokenType = ExpressionCalculator.TokenType.BitwiseOr;
                        }
                        break;
                    default:
                        throw this.unexpectedCharacterCalcException(this._nextPosition);
                }
                this._token = this._expression.substring(this._position, this._nextPosition + 1);
                this._nextPosition++;
                this.skipWhiteSpace();
            }
        }
    }

    private unexpectedCharacterCalcException(pos: number): CalcException {
        const charAsInt = this._expression.charCodeAt(pos);
        const hexOfChar = charAsInt.toString(16).toUpperCase();
        const charAsString = this._expression.substring(pos, pos + 1);
        const message = charAsInt < 127
            ? `Unexpected character at position ${pos} ('${charAsString}', 0x${hexOfChar}); see: \n${this._expression}.`
            : `Unexpected character at position ${pos} (0x${hexOfChar}); see:\n\`${this._expression}\``;
        return new CalcException(message, this._expression, pos);
    }

    private skipWhiteSpace(): void {
        while (this._nextPosition < this._expression.length && /\s/.test(this._expression[this._nextPosition])) {
            this._nextPosition++;
        }
    }

    private functionFromString(name: string): ExpressionCalculator.Function | null {
        name = name.toUpperCase();
        return this._functions.find(func => func.name === name) || null;
    }

    private constantFromString(name: string): ExpressionCalculator.Constant | null {
        name = name.toUpperCase();
        return this._constants.find(constant => constant.name === name) || null;
    }

    private variableFromString(name: string): ExpressionCalculator.Variable | null {
        name = name.toUpperCase();
        return this._variables.find(variable => variable.name === name) || null;
    }

    private exact(d: number): number {
        if (d >= -Number.MAX_VALUE && d <= Number.MAX_VALUE) {
            return d;
        }
        throw new CalcException('Result to large. Conversion impossible.', this._expression, this._position);
    }

    private round(d: number, dPrecision: number): number {
        return Math.round(d / dPrecision) * dPrecision;
    }

    private isLetter(char: string): boolean {
        return /[a-zA-Z]/.test(char);
    }

    private isDigit(char: string): boolean {
        return /[0-9]/.test(char);
    }

    // Math functions
    private abs(): any {
        if (this._tokenType !== ExpressionCalculator.TokenType.OpenParen) {
            throw new CalcException("Expected '('", this._expression, this._position);
        }
        this.nextToken();

        const value = Math.abs(this.expression());
        if (this._tokenType !== ExpressionCalculator.TokenType.CloseParen) {
            throw new CalcException("Expected ')'", this._expression, this._position);
        }
        this.nextToken();

        return value;
    }

    private acos(): any {
        if (this._tokenType !== ExpressionCalculator.TokenType.OpenParen) {
            throw new CalcException("Expected '('", this._expression, this._position);
        }
        this.nextToken();

        const value = Math.acos(this.expression());
        if (this._tokenType !== ExpressionCalculator.TokenType.CloseParen) {
            throw new CalcException("Expected ')'", this._expression, this._position);
        }
        this.nextToken();

        return value;
    }

    private asin(): any {
        if (this._tokenType !== ExpressionCalculator.TokenType.OpenParen) {
            throw new CalcException("Expected '('", this._expression, this._position);
        }
        this.nextToken();

        const value = Math.asin(this.expression());
        if (this._tokenType !== ExpressionCalculator.TokenType.CloseParen) {
            throw new CalcException("Expected ')'", this._expression, this._position);
        }
        this.nextToken();

        return value;
    }

    private atan(): any {
        if (this._tokenType !== ExpressionCalculator.TokenType.OpenParen) {
            throw new CalcException("Expected '('", this._expression, this._position);
        }
        this.nextToken();

        const value = Math.atan(this.expression());
        if (this._tokenType !== ExpressionCalculator.TokenType.CloseParen) {
            throw new CalcException("Expected ')'", this._expression, this._position);
        }
        this.nextToken();

        return value;
    }

    private atan2(): any {
        if (this._tokenType !== ExpressionCalculator.TokenType.OpenParen) {
            throw new CalcException("Expected '('", this._expression, this._position);
        }
        this.nextToken();

        const value1 = this.expression();
        if (this._tokenType !== ExpressionCalculator.TokenType.Comma) {
            throw new CalcException('expected comma', this._expression, this._position);
        }
        this.nextToken();

        const value2 = this.expression();
        const value = Math.atan2(value1, value2);
        if (this._tokenType !== ExpressionCalculator.TokenType.CloseParen) {
            throw new CalcException("Expected ')'", this._expression, this._position);
        }
        this.nextToken();

        return value;
    }

    private ceil(): any {
        if (this._tokenType !== ExpressionCalculator.TokenType.OpenParen) {
            throw new CalcException("Expected '('", this._expression, this._position);
        }
        this.nextToken();

        const value = Math.ceil(this.expression());
        if (this._tokenType !== ExpressionCalculator.TokenType.CloseParen) {
            throw new CalcException("Expected ')'", this._expression, this._position);
        }
        this.nextToken();

        return value;
    }

    private cos(): any {
        if (this._tokenType !== ExpressionCalculator.TokenType.OpenParen) {
            throw new CalcException("Expected '('", this._expression, this._position);
        }
        this.nextToken();

        const value = Math.cos(this.expression());
        if (this._tokenType !== ExpressionCalculator.TokenType.CloseParen) {
            throw new CalcException("Expected ')'", this._expression, this._position);
        }
        this.nextToken();

        return value;
    }

    private cosh(): any {
        if (this._tokenType !== ExpressionCalculator.TokenType.OpenParen) {
            throw new CalcException("Expected '('", this._expression, this._position);
        }
        this.nextToken();

        const value = Math.cosh(this.expression());
        if (this._tokenType !== ExpressionCalculator.TokenType.CloseParen) {
            throw new CalcException("Expected ')'", this._expression, this._position);
        }
        this.nextToken();

        return value;
    }

    private exp(): any {
        if (this._tokenType !== ExpressionCalculator.TokenType.OpenParen) {
            throw new CalcException("Expected '('", this._expression, this._position);
        }
        this.nextToken();

        const value = Math.exp(this.expression());
        if (this._tokenType !== ExpressionCalculator.TokenType.CloseParen) {
            throw new CalcException("Expected ')'", this._expression, this._position);
        }
        this.nextToken();

        return value;
    }

    private floor(): any {
        if (this._tokenType !== ExpressionCalculator.TokenType.OpenParen) {
            throw new CalcException("Expected '('", this._expression, this._position);
        }
        this.nextToken();

        const value = Math.floor(this.expression());
        if (this._tokenType !== ExpressionCalculator.TokenType.CloseParen) {
            throw new CalcException("Expected ')'", this._expression, this._position);
        }
        this.nextToken();

        return value;
    }

    private log(): any {
        if (this._tokenType !== ExpressionCalculator.TokenType.OpenParen) {
            throw new CalcException("Expected '('", this._expression, this._position);
        }
        this.nextToken();

        const value = Math.log(this.expression());
        if (this._tokenType !== ExpressionCalculator.TokenType.CloseParen) {
            throw new CalcException("Expected ')'", this._expression, this._position);
        }
        this.nextToken();

        return value;
    }

    private log10(): any {
        if (this._tokenType !== ExpressionCalculator.TokenType.OpenParen) {
            throw new CalcException("Expected '('", this._expression, this._position);
        }
        this.nextToken();

        const value = Math.log10(this.expression());
        if (this._tokenType !== ExpressionCalculator.TokenType.CloseParen) {
            throw new CalcException("Expected ')'", this._expression, this._position);
        }
        this.nextToken();

        return value;
    }

    private sin(): any {
        if (this._tokenType !== ExpressionCalculator.TokenType.OpenParen) {
            throw new CalcException("Expected '('", this._expression, this._position);
        }
        this.nextToken();

        const value = Math.sin(this.expression());
        if (this._tokenType !== ExpressionCalculator.TokenType.CloseParen) {
            throw new CalcException("Expected ')'", this._expression, this._position);
        }
        this.nextToken();

        return value;
    }

    private sinh(): any {
        if (this._tokenType !== ExpressionCalculator.TokenType.OpenParen) {
            throw new CalcException("Expected '('", this._expression, this._position);
        }
        this.nextToken();

        const value = Math.sinh(this.expression());
        if (this._tokenType !== ExpressionCalculator.TokenType.CloseParen) {
            throw new CalcException("Expected ')'", this._expression, this._position);
        }
        this.nextToken();

        return value;
    }

    private sqrt(): any {
        if (this._tokenType !== ExpressionCalculator.TokenType.OpenParen) {
            throw new CalcException("Expected '('", this._expression, this._position);
        }
        this.nextToken();

        const value = Math.sqrt(this.expression());
        if (this._tokenType !== ExpressionCalculator.TokenType.CloseParen) {
            throw new CalcException("Expected ')'", this._expression, this._position);
        }
        this.nextToken();

        return value;
    }

    private tan(): any {
        if (this._tokenType !== ExpressionCalculator.TokenType.OpenParen) {
            throw new CalcException("Expected '('", this._expression, this._position);
        }
        this.nextToken();

        const value = Math.tan(this.expression());
        if (this._tokenType !== ExpressionCalculator.TokenType.CloseParen) {
            throw new CalcException("Expected ')'", this._expression, this._position);
        }
        this.nextToken();

        return value;
    }

    private tanh(): any {
        if (this._tokenType !== ExpressionCalculator.TokenType.OpenParen) {
            throw new CalcException("Expected '('", this._expression, this._position);
        }
        this.nextToken();

        const value = Math.tanh(this.expression());
        if (this._tokenType !== ExpressionCalculator.TokenType.CloseParen) {
            throw new CalcException("Expected ')'", this._expression, this._position);
        }
        this.nextToken();

        return value;
    }

    private truncate(): any {
        if (this._tokenType !== ExpressionCalculator.TokenType.OpenParen) {
            throw new CalcException("Expected '('", this._expression, this._position);
        }
        this.nextToken();

        const value = Math.trunc(this.expression());
        if (this._tokenType !== ExpressionCalculator.TokenType.CloseParen) {
            throw new CalcException("Expected ')'", this._expression, this._position);
        }
        this.nextToken();

        return value;
    }

    private max(): any {
        if (this._tokenType !== ExpressionCalculator.TokenType.OpenParen) {
            throw new CalcException("Expected '('", this._expression, this._position);
        }
        this.nextToken();

        const value1 = this.expression();
        if (this._tokenType !== ExpressionCalculator.TokenType.Comma) {
            throw new CalcException('expected comma', this._expression, this._position);
        }
        this.nextToken();

        const value2 = this.expression();
        const value = Math.max(value1, value2);
        if (this._tokenType !== ExpressionCalculator.TokenType.CloseParen) {
            throw new CalcException("Expected ')'", this._expression, this._position);
        }
        this.nextToken();

        return value;
    }

    private min(): any {
        if (this._tokenType !== ExpressionCalculator.TokenType.OpenParen) {
            throw new CalcException("Expected '('", this._expression, this._position);
        }
        this.nextToken();

        const value1 = this.expression();
        if (this._tokenType !== ExpressionCalculator.TokenType.Comma) {
            throw new CalcException('expected comma', this._expression, this._position);
        }
        this.nextToken();

        const value2 = this.expression();
        const value = Math.min(value1, value2);
        if (this._tokenType !== ExpressionCalculator.TokenType.CloseParen) {
            throw new CalcException("Expected ')'", this._expression, this._position);
        }
        this.nextToken();

        return value;
    }

    // String functions
    private tolower(): any {
        if (this._tokenType !== ExpressionCalculator.TokenType.OpenParen) {
            throw new CalcException("Expected '('", this._expression, this._position);
        }
        this.nextToken();

        const value = this.expression();
        if (typeof value !== 'string') {
            throw new CalcException('Expected string', this._expression, this._position);
        }

        if (this._tokenType !== ExpressionCalculator.TokenType.CloseParen) {
            throw new CalcException("Expected ')'", this._expression, this._position);
        }
        this.nextToken();

        return value.toLowerCase();
    }

    private toupper(): any {
        if (this._tokenType !== ExpressionCalculator.TokenType.OpenParen) {
            throw new CalcException("Expected '('", this._expression, this._position);
        }
        this.nextToken();

        const value = this.expression();
        if (typeof value !== 'string') {
            throw new CalcException('Expected string', this._expression, this._position);
        }

        if (this._tokenType !== ExpressionCalculator.TokenType.CloseParen) {
            throw new CalcException("Expected ')'", this._expression, this._position);
        }
        this.nextToken();

        return value.toUpperCase();
    }

    private equals(): any {
        if (this._tokenType !== ExpressionCalculator.TokenType.OpenParen) {
            throw new CalcException("Expected '('", this._expression, this._position);
        }
        this.nextToken();

        const value1 = this.expression();
        if (typeof value1 !== 'string') {
            throw new CalcException('Expected string', this._expression, this._position);
        }

        if (this._tokenType !== ExpressionCalculator.TokenType.Comma) {
            throw new CalcException('expected comma', this._expression, this._position);
        }
        this.nextToken();

        const value2 = this.expression();
        if (typeof value2 !== 'string') {
            throw new CalcException('Expected string', this._expression, this._position);
        }

        const value = value1 === value2;
        if (this._tokenType !== ExpressionCalculator.TokenType.CloseParen) {
            throw new CalcException("Expected ')'", this._expression, this._position);
        }
        this.nextToken();

        return value;
    }

    private contains(): any {
        if (this._tokenType !== ExpressionCalculator.TokenType.OpenParen) {
            throw new CalcException("Expected '('", this._expression, this._position);
        }
        this.nextToken();

        const value1 = this.expression();
        if (typeof value1 !== 'string') {
            throw new CalcException('Expected string', this._expression, this._position);
        }

        if (this._tokenType !== ExpressionCalculator.TokenType.Comma) {
            throw new CalcException('expected comma', this._expression, this._position);
        }
        this.nextToken();

        const value2 = this.expression();
        if (typeof value2 !== 'string') {
            throw new CalcException('Expected string', this._expression, this._position);
        }

        const value = value1.includes(value2);
        if (this._tokenType !== ExpressionCalculator.TokenType.CloseParen) {
            throw new CalcException("Expected ')'", this._expression, this._position);
        }
        this.nextToken();

        return value;
    }

    private startsWith(): any {
        if (this._tokenType !== ExpressionCalculator.TokenType.OpenParen) {
            throw new CalcException("Expected '('", this._expression, this._position);
        }
        this.nextToken();

        const value1 = this.expression();
        if (typeof value1 !== 'string') {
            throw new CalcException('Expected string', this._expression, this._position);
        }

        if (this._tokenType !== ExpressionCalculator.TokenType.Comma) {
            throw new CalcException('expected comma', this._expression, this._position);
        }
        this.nextToken();

        const value2 = this.expression();
        if (typeof value2 !== 'string') {
            throw new CalcException('Expected string', this._expression, this._position);
        }

        const value = value1.startsWith(value2);
        if (this._tokenType !== ExpressionCalculator.TokenType.CloseParen) {
            throw new CalcException("Expected ')'", this._expression, this._position);
        }
        this.nextToken();

        return value;
    }

    private endsWith(): any {
        if (this._tokenType !== ExpressionCalculator.TokenType.OpenParen) {
            throw new CalcException("Expected '('", this._expression, this._position);
        }
        this.nextToken();

        const value1 = this.expression();
        if (typeof value1 !== 'string') {
            throw new CalcException('Expected string', this._expression, this._position);
        }

        if (this._tokenType !== ExpressionCalculator.TokenType.Comma) {
            throw new CalcException('expected comma', this._expression, this._position);
        }
        this.nextToken();

        const value2 = this.expression();
        if (typeof value2 !== 'string') {
            throw new CalcException('Expected string', this._expression, this._position);
        }

        const value = value1.endsWith(value2);
        if (this._tokenType !== ExpressionCalculator.TokenType.CloseParen) {
            throw new CalcException("Expected ')'", this._expression, this._position);
        }
        this.nextToken();

        return value;
    }

    private isEmpty(): any {
        if (this._tokenType !== ExpressionCalculator.TokenType.OpenParen) {
            throw new CalcException("Expected '('", this._expression, this._position);
        }
        this.nextToken();

        const value = this.expression();
        if (typeof value !== 'string') {
            throw new CalcException('Expected string', this._expression, this._position);
        }

        const result = value === null || value === undefined || value === '';
        if (this._tokenType !== ExpressionCalculator.TokenType.CloseParen) {
            throw new CalcException("Expected ')'", this._expression, this._position);
        }
        this.nextToken();

        return result;
    }

    /**
     * Converts a number to string representation in the specified radix
     * @param d The number to convert
     * @param radix The radix to use for conversion
     * @returns String representation of the number in the specified radix
     */
    public strFromDRadix(d: number, radix: ExpressionCalculator.Radix): string {
        if (radix === ExpressionCalculator.Radix.Dec) {
            return d.toString();
        } else if (d >= Number.MIN_SAFE_INTEGER && d <= Number.MAX_SAFE_INTEGER) {
            let szValue: string;
            if (radix > ExpressionCalculator.Radix.Dec) {
                szValue = '0' + Math.floor(d).toString(radix);
            } else {
                szValue = Math.floor(d).toString(radix);
            }
            szValue = szValue.toUpperCase();
            switch (radix) {
                case ExpressionCalculator.Radix.Hex:
                    szValue += 'h';
                    break;
                case ExpressionCalculator.Radix.Oct:
                    szValue += 'o';
                    break;
                case ExpressionCalculator.Radix.Bin:
                    szValue += 'b';
                    break;
            }
            return szValue;
        } else {
            throw new CalcException('Result to large. ExpressionCalculator.Radix conversion impossible.', this._expression, this._position);
        }
    }
}

/**
 * Namespace for ExpressionCalculator inner classes
 */
export namespace ExpressionCalculator {
    /**
     * Function definition
     */
    export class Function {
        public name: string;
        public delegate: () => any;

        constructor(name: string, functionDelegate: () => any) {
            this.name = name;
            this.delegate = functionDelegate;
        }
    }

    /**
     * Constant definition
     */
    export class Constant {
        public name: string;
        public value: any;

        constructor(name: string, value: any) {
            this.name = name;
            this.value = value;
        }
    }

    /**
     * Variable definition
     */
    export class Variable {
        public name: string;
        public value: any; // Could be number, string, or boolean

        constructor(name: string, value: any) {
            this.name = name;
            this.value = value;
        }
    }
}