using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

public class Employee
{
    public int Id { get; set; }                 // Column 1
    public string FirstName { get; set; }       // Column 2
    public string LastName { get; set; }        // Column 3
    public string Department { get; set; }      // Column 4
    public string JobTitle { get; set; }        // Column 5
    public decimal MonthlySalary { get; set; }  // Column 6
    public double TaxRate { get; set; }         // Column 7
    public DateTime HireDate { get; set; }      // Column 8
    public bool IsActive { get; set; }          // Column 9
    public string Email { get; set; }           // Column 10

    // Computed values (not stored columns)
    public decimal NetSalary => MonthlySalary - (MonthlySalary * (decimal)TaxRate);
}

class Program
{
    // The system starts with an EMPTY list (0 pre-defined records)
    static List<Employee> employees = new List<Employee>();
    static int nextId = 1001;
    static string appTitle = "EMPLOYEE PAYROLL & HR MANAGEMENT SYSTEM (EPHRS)";

    static void Main(string[] args)
    {
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

        bool running = true;
        while (running)
        {
            Console.Clear();
            Console.WriteLine($"=== {appTitle} ===");
            Console.WriteLine("1. Create (Add Employee)");
            Console.WriteLine("2. Read (View All Employees)");
            Console.WriteLine("3. Search / Filter Employees");
            Console.WriteLine("4. Update Employee");
            Console.WriteLine("5. Delete Employee");
            Console.WriteLine("6. Exit");
            Console.Write("\nSelect an option: ");
            string choice = Console.ReadLine();
            Console.WriteLine();

            switch (choice)
            {
                case "1": CreateEmployee(); break;
                case "2": ReadEmployees(); break;
                case "3": SearchEmployees(); break;
                case "4": UpdateEmployee(); break;
                case "5": DeleteEmployee(); break;
                case "6": running = false; break;
                default: Console.WriteLine("Invalid choice."); break;
            }

            if (running) { Console.WriteLine("\nPress Enter to continue..."); Console.ReadLine(); }
        }
    }

    // ---------------------------------------------------------------- CREATE
    static void CreateEmployee()
    {
        Console.WriteLine("--- Add New Employee ---");
        Console.WriteLine($"Employee ID (auto): {nextId}");

        Employee emp = new Employee
        {
            Id = nextId,
            FirstName = ReadRequired("Enter First Name: "),
            LastName = ReadRequired("Enter Last Name: "),
            Department = ReadRequired("Enter Department (IT, HR, Sales, Finance): "),
            JobTitle = ReadRequired("Enter Job Title: "),
            MonthlySalary = ReadSalary("Enter Monthly Salary: "),
            TaxRate = ReadTaxRate("Enter Tax Rate (e.g., 0.12 for 12%): "),
            HireDate = ReadDate("Enter Hire Date (yyyy-MM-dd): "),
            IsActive = ReadYesNo("Is the employee active? (Y/N): "),
            Email = ReadEmail("Enter Email: ")
        };

        employees.Add(emp);
        nextId++;

        Console.WriteLine($"\nEmployee {emp.Id} added successfully!");
    }

    // ------------------------------------------------------------------ READ
    static void ReadEmployees()
    {
        Console.WriteLine("--- Employee Records ---");
        if (employees.Count == 0)
        {
            Console.WriteLine("No employees found. (0 records)");
            return;
        }

        foreach (Employee e in employees.OrderBy(x => x.Id))
            PrintEmployee(e);

        Console.WriteLine(new string('-', 70));
        Console.WriteLine($"Total Employees : {employees.Count}  (Active: {employees.Count(e => e.IsActive)}, Inactive: {employees.Count(e => !e.IsActive)})");
        Console.WriteLine($"Total Gross Pay : PHP {employees.Sum(e => e.MonthlySalary):N2}");
        Console.WriteLine($"Total Net Pay   : PHP {employees.Sum(e => e.NetSalary):N2}");
        Console.WriteLine($"Average Salary  : PHP {employees.Average(e => e.MonthlySalary):N2}");
    }

    // ---------------------------------------------------------------- SEARCH
    static void SearchEmployees()
    {
        Console.WriteLine("--- Search / Filter Employees ---");
        if (employees.Count == 0) { Console.WriteLine("No employees found. (0 records)"); return; }

        Console.WriteLine("1. By Name (first or last)");
        Console.WriteLine("2. By Department");
        Console.WriteLine("3. By Employment Status");
        Console.WriteLine("4. By Salary Range");
        Console.Write("Select search type: ");
        string type = Console.ReadLine();

        List<Employee> results;
        switch (type)
        {
            case "1":
                string keyword = ReadRequired("Enter name keyword: ");
                results = employees.Where(e =>
                    e.FirstName.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    e.LastName.Contains(keyword, StringComparison.OrdinalIgnoreCase)).ToList();
                break;
            case "2":
                string dept = ReadRequired("Enter department: ");
                results = employees.Where(e =>
                    string.Equals(e.Department, dept, StringComparison.OrdinalIgnoreCase)).ToList();
                break;
            case "3":
                bool active = ReadYesNo("Show active employees? (Y = Active, N = Inactive): ");
                results = employees.Where(e => e.IsActive == active).ToList();
                break;
            case "4":
                decimal min = ReadSalary("Enter minimum salary: ");
                decimal max = ReadSalary("Enter maximum salary: ");
                results = employees.Where(e => e.MonthlySalary >= min && e.MonthlySalary <= max)
                                   .OrderBy(e => e.MonthlySalary).ToList();
                break;
            default:
                Console.WriteLine("\nInvalid search type.");
                return;
        }

        Console.WriteLine($"\n--- Search Results: {results.Count} record(s) found ---");
        foreach (Employee e in results) PrintEmployee(e);
    }

    // ---------------------------------------------------------------- UPDATE
    static void UpdateEmployee()
    {
        ReadEmployees();
        if (employees.Count == 0) return;

        Console.Write("\nEnter the ID of the employee to update: ");
        if (!int.TryParse(Console.ReadLine(), out int id))
        {
            Console.WriteLine("\nInvalid ID format.");
            return;
        }

        Employee emp = employees.FirstOrDefault(e => e.Id == id);
        if (emp == null)
        {
            Console.WriteLine("\nEmployee not found.");
            return;
        }

        Console.WriteLine("\n(Press Enter to keep the current value)");

        string v = Prompt($"First name [{emp.FirstName}]: ");
        if (!string.IsNullOrWhiteSpace(v)) emp.FirstName = v;

        v = Prompt($"Last name [{emp.LastName}]: ");
        if (!string.IsNullOrWhiteSpace(v)) emp.LastName = v;

        v = Prompt($"Department [{emp.Department}]: ");
        if (!string.IsNullOrWhiteSpace(v)) emp.Department = v;

        v = Prompt($"Job title [{emp.JobTitle}]: ");
        if (!string.IsNullOrWhiteSpace(v)) emp.JobTitle = v;

        v = Prompt($"Monthly salary [{emp.MonthlySalary:F2}]: ");
        if (decimal.TryParse(v, out decimal salary) && salary > 0) emp.MonthlySalary = salary;

        v = Prompt($"Tax rate [{emp.TaxRate}]: ");
        if (double.TryParse(v, out double tax) && tax >= 0 && tax <= 1) emp.TaxRate = tax;

        v = Prompt($"Hire date [{emp.HireDate:yyyy-MM-dd}]: ");
        if (DateTime.TryParseExact(v, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime hired))
            emp.HireDate = hired;

        v = Prompt($"Active (Y/N) [{(emp.IsActive ? "Y" : "N")}]: ");
        if (v.Trim().ToUpper() == "Y") emp.IsActive = true;
        else if (v.Trim().ToUpper() == "N") emp.IsActive = false;

        v = Prompt($"Email [{emp.Email}]: ");
        if (v.Contains("@")) emp.Email = v.Trim();

        Console.WriteLine("\nEmployee updated successfully!");
    }

    // ---------------------------------------------------------------- DELETE
    static void DeleteEmployee()
    {
        ReadEmployees();
        if (employees.Count == 0) return;

        Console.Write("\nEnter the ID of the employee to delete: ");
        if (!int.TryParse(Console.ReadLine(), out int id))
        {
            Console.WriteLine("\nInvalid ID format.");
            return;
        }

        Employee emp = employees.FirstOrDefault(e => e.Id == id);
        if (emp == null)
        {
            Console.WriteLine("\nEmployee not found.");
            return;
        }

        if (ReadYesNo($"Delete {emp.FirstName} {emp.LastName} (ID {emp.Id})? (Y/N): "))
        {
            employees.RemoveAll(e => e.Id == id);
            Console.WriteLine("\nEmployee deleted successfully!");
        }
        else
        {
            Console.WriteLine("\nDelete cancelled.");
        }
    }

    // --------------------------------------------------------------- HELPERS
    static void PrintEmployee(Employee e)
    {
        string status = e.IsActive ? "Active" : "On Leave/Inactive";
        Console.WriteLine($"[{e.Id}] {e.FirstName} {e.LastName}  |  {e.Department} - {e.JobTitle}  |  {status}");
        Console.WriteLine($"       Salary: PHP {e.MonthlySalary:N2}   Tax: {e.TaxRate * 100:0.##}%   Net: PHP {e.NetSalary:N2}");
        Console.WriteLine($"       Hired: {e.HireDate:yyyy-MM-dd}   Email: {e.Email}");
        Console.WriteLine();
    }

    static string Prompt(string text)
    {
        Console.Write(text);
        return Console.ReadLine() ?? "";
    }

    static string ReadRequired(string text)
    {
        while (true)
        {
            string input = Prompt(text).Trim();
            if (!string.IsNullOrWhiteSpace(input)) return input;
            Console.WriteLine("  Value is required. Please try again.");
        }
    }

    static decimal ReadSalary(string text)
    {
        while (true)
        {
            if (decimal.TryParse(Prompt(text), out decimal value) && value > 0) return value;
            Console.WriteLine("  Enter a valid amount greater than 0.");
        }
    }

    static double ReadTaxRate(string text)
    {
        while (true)
        {
            if (double.TryParse(Prompt(text), out double value))
            {
                if (value > 1) value /= 100;           // allow 12 to mean 12%
                if (value >= 0 && value <= 1) return value;
            }
            Console.WriteLine("  Enter a rate between 0 and 1 (e.g., 0.12).");
        }
    }

    static DateTime ReadDate(string text)
    {
        while (true)
        {
            if (DateTime.TryParseExact(Prompt(text), "yyyy-MM-dd", CultureInfo.InvariantCulture,
                                       DateTimeStyles.None, out DateTime value))
                return value;
            Console.WriteLine("  Use the format yyyy-MM-dd (e.g., 2024-06-15).");
        }
    }

    static bool ReadYesNo(string text)
    {
        while (true)
        {
            string input = Prompt(text).Trim().ToUpper();
            if (input == "Y") return true;
            if (input == "N") return false;
            Console.WriteLine("  Please enter Y or N.");
        }
    }

    static string ReadEmail(string text)
    {
        while (true)
        {
            string input = Prompt(text).Trim();
            if (input.Contains("@") && input.Contains(".")) return input;
            Console.WriteLine("  Enter a valid email address.");
        }
    }
}
