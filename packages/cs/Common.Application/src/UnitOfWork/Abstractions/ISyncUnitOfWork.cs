namespace Common.Application.UnitOfWork.Abstractions;

public interface ISyncUnitOfWork : IDisposable
{
    T Exec<T>(Func<T> block);
    void Exec(Action block);
    T ExecNoTracking<T>(Func<T> block);
    void ExecNoTracking(Action block);
    T ExecNoTrackingWithIdentityResolution<T>(Func<T> block);
    void ExecNoTrackingWithIdentityResolution(Action block);
    T ExecTracking<T>(Func<T> block);
    void ExecTracking(Action block);
    long SaveChanges();
}
