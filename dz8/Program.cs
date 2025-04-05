namespace dz8
{
    internal class Program
    {
        delegate int StringProcessor(string input);
        delegate bool NumberFilter(int number);
        public class CreditCard
        {
            public string CardNumber { get; }
            public string OwnerName { get; set; }
            public DateTime ExpiryDate { get; }
            private int _pin;
            public decimal CreditLimit { get; set; }
            private decimal _balance;

            public event Action<decimal> BalanceToppedUp;
            public event Action<decimal> MoneySpent;
            public event Action CreditUsageStarted;
            public event Action<decimal> TargetAmountReached;
            public event Action PinChanged;

            public CreditCard(string cardNumber, string ownerName, DateTime expiryDate, int pin, decimal creditLimit)
            {
                CardNumber = cardNumber;
                OwnerName = ownerName;
                ExpiryDate = expiryDate;
                _pin = pin;
                CreditLimit = creditLimit;
                _balance = 0;
            }

            public void TopUp(decimal amount)
            {
                if (amount <= 0)
                    throw new ArgumentException("Suma popolnennia maje buty bilshe 0");

                _balance += amount;
                BalanceToppedUp?.Invoke(amount);

                CheckTargetAmount();
            }

            public bool SpendMoney(decimal amount, int enteredPin)
            {
                if (!VerifyPin(enteredPin))
                    return false;

                if (amount <= 0)
                    throw new ArgumentException("Suma vytraty maje buty bilshe 0");

                bool usedCredit = false;

                if (_balance >= amount)
                {
                    _balance -= amount;
                }
                else
                {
                    decimal remaining = amount - _balance;
                    if (remaining <= CreditLimit)
                    {
                        _balance = -remaining;
                        usedCredit = true;
                        CreditUsageStarted?.Invoke();
                    }
                    else
                    {
                        return false;
                    }
                }

                MoneySpent?.Invoke(amount);
                CheckTargetAmount();

                return true;
            }

            public void ChangePin(int oldPin, int newPin)
            {
                if (!VerifyPin(oldPin))
                    throw new InvalidOperationException("Nevirnyi staryi PIN");

                _pin = newPin;
                PinChanged?.Invoke();
            }

            private bool VerifyPin(int pin) => _pin == pin;

            private void CheckTargetAmount()
            {
                const decimal targetAmount = 1000m;
                if (_balance >= targetAmount)
                {
                    TargetAmountReached?.Invoke(_balance);
                }
            }

            public decimal GetBalance() => _balance;
        }

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.Unicode;
            // task 1
            int[] numbers = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };

            var evenNumbers = FilterNumbers(numbers, IsEven);
            Console.WriteLine("Even numbers: " + string.Join(", ", evenNumbers));

            var oddNumbers = FilterNumbers(numbers, IsOdd);
            Console.WriteLine("Odd numbers: " + string.Join(", ", oddNumbers));

            var primes = FilterNumbers(numbers, IsPrime);
            Console.WriteLine("Prime numbers: " + string.Join(", ", primes));

            var fibonacciNumbers = FilterNumbers(numbers, IsFibonacci);
            Console.WriteLine("Fibonacci numbers: " + string.Join(", ", fibonacciNumbers));

            // task 2
            Action showCurrentTime = () => Console.WriteLine("Current time: " + DateTime.Now.ToString("HH:mm:ss"));
            Action showCurrentDate = () => Console.WriteLine("Current date: " + DateTime.Now.ToString("yyyy-MM-dd"));
            Action showCurrentDayOfWeek = () => Console.WriteLine("Current day of week: " + DateTime.Now.DayOfWeek);

            Func<double, double, double, double> triangleArea = (a, b, c) =>
            {
                double p = (a + b + c) / 2;
                return Math.Sqrt(p * (p - a) * (p - b) * (p - c));
            };

            Func<double, double, double> rectangleArea = (width, height) => width * height;

            Predicate<double[]> isRightTriangle = sides =>
            {
                if (sides.Length != 3) return false;
                Array.Sort(sides);
                return Math.Abs(Math.Pow(sides[0], 2) + Math.Pow(sides[1], 2) - Math.Pow(sides[2], 2)) < 0.0001;
            };

            showCurrentTime();
            showCurrentDate();
            showCurrentDayOfWeek();

            double triangle = triangleArea(3, 4, 5);
            Console.WriteLine($"Area of triangle with sides 3, 4, 5: {triangle}");

            double rectangle = rectangleArea(4, 5);
            Console.WriteLine($"Area of rectangle 4x5: {rectangle}");

            double[] triangleSides = { 3, 4, 5 };
            Console.WriteLine($"Triangle with sides 3, 4, 5 is right: {isRightTriangle(triangleSides)}");

            // task 3
            var card = new CreditCard("1234 5678 9012 3456", "Ivanov Ivan Ivanovych",
                                 new DateTime(2025, 12, 31), 1234, 5000);

            card.BalanceToppedUp += amount => Console.WriteLine($"Rakhunok popolneno na {amount} hrn");
            card.MoneySpent += amount => Console.WriteLine($"Zniato {amount} hrn z rakhunku");
            card.CreditUsageStarted += () => Console.WriteLine("Pochatok vykorystannia kredytnykh koshtiv");
            card.TargetAmountReached += balance => Console.WriteLine($"Dosiahnuto tsilovu sumu! Potochnyi balans: {balance} hrn");
            card.PinChanged += () => Console.WriteLine("PIN uspishno zmineno");

            card.TopUp(500);
            card.TopUp(600);
            card.SpendMoney(200, 1234);
            card.ChangePin(1234, 4321);
            card.SpendMoney(1000, 4321);

            // task 4
            string text = "Yakys tekstik";

            StringProcessor vowelCounter = CountVowels;
            StringProcessor consonantCounter = CountConsonants;
            StringProcessor lengthGetter = GetLength;

            Console.WriteLine($"Riadok: {text}");
            Console.WriteLine($"Holosnykh liter: {vowelCounter(text)}");
            Console.WriteLine($"Pryholosnykh liter: {consonantCounter(text)}");
            Console.WriteLine($"Dovzhyna riadka: {lengthGetter(text)}");

            StringProcessor multiProcessor = vowelCounter;
            multiProcessor += consonantCounter;
            multiProcessor += lengthGetter;

            Console.WriteLine("\nRezultaty bagatoadresnoho delegata:");
            foreach (StringProcessor processor in multiProcessor.GetInvocationList())
            {
                Console.WriteLine($"Rezultat: {processor(text)}");
            }
        }

        static int CountVowels(string input)
        {
            char[] vowels = { 'a', 'e', 'je', 'y', 'i', 'ji', 'o', 'u', 'ju', 'ja',
                         'A', 'E', 'Je', 'Y', 'I', 'Ji', 'O', 'U', 'Ju', 'Ja' };
            return input.Count(c => vowels.Contains(c));
        }

        static int CountConsonants(string input)
        {
            char[] consonants = { 'b', 'v', 'h', 'g', 'd', 'zh', 'z', 'j', 'k', 'l', 'm',
                              'n', 'p', 'r', 's', 't', 'f', 'kh', 'ts', 'ch', 'sh', 'shch',
                              'B', 'V', 'H', 'G', 'D', 'Zh', 'Z', 'J', 'K', 'L', 'M',
                              'N', 'P', 'R', 'S', 'T', 'F', 'Kh', 'Ts', 'Ch', 'Sh', 'Shch' };
            return input.Count(c => consonants.Contains(c));
        }

        static int GetLength(string input) => input.Length;

        static IEnumerable<int> FilterNumbers(int[] numbers, NumberFilter filter)
        {
            foreach (var number in numbers)
            {
                if (filter(number))
                {
                    yield return number;
                }
            }
        }
        static bool IsEven(int number) => number % 2 == 0;
        static bool IsOdd(int number) => number % 2 != 0;

        static bool IsPrime(int number)
        {
            if (number <= 1) return false;
            if (number == 2) return true;
            if (number % 2 == 0) return false;

            for (int i = 3; i * i <= number; i += 2)
            {
                if (number % i == 0)
                    return false;
            }
            return true;
        }

        static bool IsFibonacci(int number)
        {
            return IsPerfectSquare(5 * number * number + 4) || IsPerfectSquare(5 * number * number - 4);
        }

        static bool IsPerfectSquare(int x)
        {
            int s = (int)Math.Sqrt(x);
            return s * s == x;
        }

    }
}
