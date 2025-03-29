using FirebaseAdmin.Auth;
using FirebaseAdmin.Messaging;

namespace SupportMe.Helpers
{
    public interface IFirebaseHandler
    {
        FirebaseAuth Auth { get; }
        FirebaseMessaging Messaging { get; }
    }
    public class FirebaseHandler : IFirebaseHandler
    {
        public FirebaseAuth Auth { get; }
        public FirebaseMessaging Messaging { get; }

        public FirebaseHandler(FirebaseAuth auth, FirebaseMessaging messaging)
        {
            Auth = auth;
            Messaging = messaging;
        }
    }
}
