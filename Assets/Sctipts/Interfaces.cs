interface IHitable
{
    void OnHit();
}

interface ISink{
    void OnSink(Balls ball);
}

public interface IObserver<T>
{
    void OnNotify(object sender, T eventArgs);
}