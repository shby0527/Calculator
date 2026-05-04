# About Calc

This is a console calculator capable of arbitrary precision, supporting +, -, *, /, ^ (exponentiation), and trigonometric functions.

this program is under [![WTFPL](https://raw.githubusercontent.com/shby0527/Calculator/master/wtfpl-badge-1.png)](http://www.wtfpl.net/)

##  Project Background

This is a true story. Xiao A (my classmate) – his company held a coding competition, and his task was "Implement a calculator that supports basic arithmetic operations without using APIs like eval."

Me: So how did you do it?

Xiao A: I tried using regular expressions to match `*` and `/` first, then `+` and `-`, and then perform the calculations.

Me: Did you consider the possibility of parentheses?

Xiao A: I was just about to mention that. Yes, I thought of that. I first extract the parentheses, then recursively match and evaluate the inner expressions step by step, and then substitute the results back.

Me: Oh, that's pretty good.

Xiao A: But I found another problem. When there are too many parentheses or the nesting depth is too deep, my matching fails. Also, there are cases where parentheses are all around, with multiple levels. I wanted to ask you – do you have any better ideas?

Me: Have you tried converting the mathematical expression into postfix notation?

Xiao A: What's that?

Me: Oh, okay, let me explain it to you.