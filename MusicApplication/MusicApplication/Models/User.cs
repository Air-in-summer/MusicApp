using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MusicApplication.Models
{
    public partial class User : ObservableObject
    {
        [ObservableProperty]
        private int userId;
        [ObservableProperty]
        private string? username;
        [ObservableProperty]
        private string? email;
        [ObservableProperty]
        private string? passwordHash;
        [ObservableProperty]
        private string? passwordSalt;
        [ObservableProperty]
        private DateTime createdAt;
        [ObservableProperty]
        private string? role;
        [ObservableProperty]
        private string? coverImage;
    }
}
