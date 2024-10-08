﻿using AtmLibrary;

namespace AtmConsoleApp
{
    class Program
    {
        public delegate void AccountAction(Account account);
        public delegate void AtmAction(Account account, AutomatedTellerMachine atm);

        static void Main(string[] args)
        {
            // Initialize bank and get account list
            BankInitializer initializer = new BankInitializer();
            var accounts = initializer.GetAccounts();

            Account? currentAccount = AuthenticateUser(accounts);

            // Create delegates
            AccountAction checkBalanceDelegate = CheckBalance;
            AtmAction withdrawMoneyDelegate = WithdrawMoney;
            AccountAction depositMoneyDelegate = DepositMoney;

            // Main menu
            bool exit = false;
            while (!exit)
            {
                Console.WriteLine("\nSelect an action:");
                Console.WriteLine("1. Check balance");
                Console.WriteLine("2. Withdraw money");
                Console.WriteLine("3. Deposit money");
                Console.WriteLine("4. Transfer money");
                Console.WriteLine("5. View transaction history");
                Console.WriteLine("6. Find nearest ATMs");
                Console.WriteLine("7. Switch accounts");
                Console.WriteLine("0. Exit");

                string choice = Console.ReadLine() ?? "";

                switch (choice)
                {
                    case "1":
                        checkBalanceDelegate(currentAccount); // Using delegate
                        break;
                    case "2":
                        withdrawMoneyDelegate(currentAccount, initializer.atm); // Using delegate
                        break;
                    case "3":
                        depositMoneyDelegate(currentAccount); // Using delegate
                        break;
                    case "4":
                        TransferMoney(currentAccount, accounts);
                        break;
                    case "5":
                        ShowTransactionHistory(currentAccount);
                        break;
                    case "6":
                        FindNearestATMs(initializer.bank, initializer.atm);
                        break;
                    case "7":
                        currentAccount = AuthenticateUser(accounts); // Switch accounts
                        break;
                    case "0":
                        exit = true;
                        break;
                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        break;
                }
            }

            Console.WriteLine("Thank you for using our ATM. Goodbye!");
        }

        static Account? AuthenticateUser(Dictionary<string, Account> accounts)
        {
            Account? currentAccount = null;
            while (currentAccount == null)
            {
                try
                {
                    Console.WriteLine("Enter card number:");
                    string cardNumber = Console.ReadLine() ?? "";

                    Console.WriteLine("Enter PIN:");
                    string pinCode = Console.ReadLine() ?? "";

                    if (accounts.ContainsKey(cardNumber) && accounts[cardNumber].ValidatePin(pinCode))
                    {
                        currentAccount = accounts[cardNumber];
                        Console.Clear();
                        Console.WriteLine($"Welcome, {currentAccount.Owner}!");
                    }
                    else
                    {
                        Console.WriteLine("Incorrect card number or PIN. Please try again.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error during authentication: {ex.Message}");
                }
            }
            return currentAccount;
        }

        static void CheckBalance(Account account)
        {
            try
            {
                Console.WriteLine($"\nYour current balance: {account.Balance} UAH");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking balance: {ex.Message}");
            }
        }

        static void WithdrawMoney(Account account, AutomatedTellerMachine atm)
        {
            try
            {
                Console.WriteLine("\nEnter amount to withdraw:");
                if (decimal.TryParse(Console.ReadLine(), out decimal amount))
                {
                    if (amount <= 0)
                    {
                        Console.WriteLine("Amount must be greater than zero.");
                        return;
                    }

                    account.Withdraw(amount);
                    atm.DispenseCash(amount);
                    Console.WriteLine($"Successfully withdrew {amount} UAH. New balance: {account.Balance} UAH");
                }
                else
                {
                    Console.WriteLine("Invalid amount. Please enter a valid number.");
                }
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Operation failed: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
            }
        }

        static void DepositMoney(Account account)
        {
            try
            {
                Console.WriteLine("\nEnter amount to deposit:");
                if (decimal.TryParse(Console.ReadLine(), out decimal amount))
                {
                    if (amount <= 0)
                    {
                        Console.WriteLine("Amount must be greater than zero.");
                        return;
                    }

                    account.Deposit(amount);
                    Console.WriteLine($"Successfully deposited {amount} UAH. New balance: {account.Balance} UAH");
                }
                else
                {
                    Console.WriteLine("Invalid amount. Please enter a valid number.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during deposit: {ex.Message}");
            }
        }

        static void TransferMoney(Account currentAccount, Dictionary<string, Account> accounts)
        {
            try
            {
                Console.WriteLine("Enter card number to transfer money to:");
                string targetCardNumber = Console.ReadLine() ?? "";

                if (!accounts.ContainsKey(targetCardNumber))
                {
                    Console.WriteLine("Invalid card number. Please try again.");
                    return;
                }

                Account targetAccount = accounts[targetCardNumber];

                if (targetAccount.CardNumber == currentAccount.CardNumber)
                {
                    Console.WriteLine("You cannot transfer money to your own account.");
                    return;
                }

                Console.WriteLine("Enter amount to transfer:");
                if (decimal.TryParse(Console.ReadLine(), out decimal amount))
                {
                    if (amount <= 0)
                    {
                        Console.WriteLine("Amount must be greater than zero.");
                        return;
                    }

                    currentAccount.TransferMoney(targetAccount, amount);
                    Console.WriteLine($"Successfully transferred {amount} UAH to {targetAccount.Owner}. New balance: {currentAccount.Balance} UAH");
                }
                else
                {
                    Console.WriteLine("Invalid amount. Please enter a valid number.");
                }
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Operation failed: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
            }
        }

        static void ShowTransactionHistory(Account account)
        {
            try
            {
                var transactions = account.GetTransactionHistory();

                if (transactions.Count == 0)
                {
                    Console.WriteLine("\nYou have no transactions.");
                    return;
                }

                Console.WriteLine("\nSelect time period to filter transactions:");
                Console.WriteLine("1. Current day");
                Console.WriteLine("2. Current week");
                Console.WriteLine("3. Current month");
                Console.WriteLine("4. All transactions");
                string choice = Console.ReadLine() ?? "";

                List<Transaction> filteredTransactions;

                switch (choice)
                {
                    case "1":
                        filteredTransactions = TransactionFilters.FilterByCurrentDay(transactions);
                        break;
                    case "2":
                        filteredTransactions = TransactionFilters.FilterByCurrentWeek(transactions);
                        break;
                    case "3":
                        filteredTransactions = TransactionFilters.FilterByCurrentMonth(transactions);
                        break;
                    case "4":
                        filteredTransactions = transactions;
                        break;
                    default:
                        Console.WriteLine("Invalid choice. Showing all transactions.");
                        filteredTransactions = transactions;
                        break;
                }

                if (!filteredTransactions.Any())
                {
                    Console.WriteLine("\nNo transactions found for the selected period.");
                }
                else
                {
                    Console.WriteLine("\nTransaction history:");
                    foreach (var transaction in filteredTransactions)
                    {
                        Console.WriteLine(transaction);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching transaction history: {ex.Message}");
            }
        }

        static void FindNearestATMs(Bank bank, AutomatedTellerMachine currentATM)
        {
            try
            {
                Console.WriteLine("\nHow many nearest ATMs would you like to see?");
                if (int.TryParse(Console.ReadLine(), out int count))
                {
                    if (count <= 0)
                    {
                        Console.WriteLine("Please enter a positive number.");
                        return;
                    }

                    var nearestATMs = bank.GetNearestATMs(currentATM, count);
                    Console.WriteLine($"\nNearest {count} ATMs:");

                    foreach (var atm in nearestATMs)
                    {
                        double distance = currentATM.CalculateDistance(atm);
                        Console.WriteLine($"{atm.Name} - Distance: {distance:F2} km");
                    }
                }
                else
                {
                    Console.WriteLine("Invalid number. Please try again.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error finding nearest ATMs: {ex.Message}");
            }
        }
    }
}
