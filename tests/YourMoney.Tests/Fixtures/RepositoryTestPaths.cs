namespace YourMoney.Tests.Fixtures
{
    public static class RepositoryTestPaths
    {
        public static string FindYourMoneyRoot()
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);

            while (directory != null)
            {
                var candidate = Path.Combine(directory.FullName, "src", "YourMoney.Infrastructure", "Scripts");
                if (Directory.Exists(candidate))
                    return directory.FullName;

                directory = directory.Parent;
            }

            directory = new DirectoryInfo(Directory.GetCurrentDirectory());

            while (directory != null)
            {
                var candidate = Path.Combine(directory.FullName, "YourMoney", "src", "YourMoney.Infrastructure", "Scripts");
                if (Directory.Exists(candidate))
                    return Path.Combine(directory.FullName, "YourMoney");

                candidate = Path.Combine(directory.FullName, "src", "YourMoney.Infrastructure", "Scripts");
                if (Directory.Exists(candidate))
                    return directory.FullName;

                directory = directory.Parent;
            }

            throw new DirectoryNotFoundException("Could not locate the YourMoney repository root.");
        }

        public static string InYourMoney(params string[] segments)
            => Path.Combine(new[] { FindYourMoneyRoot() }.Concat(segments).ToArray());
    }
}
