using System;

namespace ChatApp.Domain.Entities;

    //constructor
    public class User
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        public string Email { get; private set; }
        public string UserName { get; private set; }
        public string PasswordHash { get; private set; }
        public DateTime CreatedAt { get; private set; }

        public User(string name, string email, string userName, string passwordHash)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be empty.", nameof(name));

            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be empty.", nameof(email));

            if (string.IsNullOrWhiteSpace(userName))
                throw new ArgumentException("UserName cannot be empty.", nameof(userName));

            if (string.IsNullOrWhiteSpace(passwordHash))
                throw new ArgumentException("PasswordHash cannot be empty.", nameof(passwordHash));

            Name = name.Trim();
            Email = email.Trim();
            UserName = userName.Trim();
            PasswordHash = passwordHash;
            CreatedAt = DateTime.UtcNow;
        }

        ////parameterless constructor (constructor rỗng) => không dùng cho business logic, dùng cho Entity Framework Core.
        ////Persistence Ignorance + Encapsulation pattern
        private User() { }

        //behavior
        public void UpdateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be empty.", nameof(name));

            Name = name.Trim();
        }

        public void ChangePassword(string passwordHash)
        {
            if (string.IsNullOrWhiteSpace(passwordHash))
                throw new ArgumentException("PasswordHash cannot be empty.", nameof(passwordHash));

            PasswordHash = passwordHash;
        }

        public void UpdateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be empty.", nameof(email));

            Email = email.Trim();
        }

        public void UpdateUserName(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
                throw new ArgumentException("UserName cannot be empty.", nameof(userName));

            UserName = userName.Trim();
        }
    }
