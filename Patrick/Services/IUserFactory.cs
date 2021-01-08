using Patrick.Models;

namespace Patrick.Services
{
    interface IUserFactory
    {
        IUser Create(ulong requestorId);
    }
}
