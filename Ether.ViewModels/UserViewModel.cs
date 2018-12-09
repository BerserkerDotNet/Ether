namespace Ether.ViewModels
{
    public class UserViewModel : ViewModelWithId
    {
        public static UserViewModel Anonymous => new UserViewModel { DisplayName = "Anonymous" };

        public string DisplayName { get; set; }

        public string Email { get; set; }
    }
}
