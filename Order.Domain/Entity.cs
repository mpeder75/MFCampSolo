namespace Order.Domain;

public abstract class Entity
{
    public abstract object GetIdentity();

    public virtual bool IsTransient()
    {
        var identity = GetIdentity();
        return identity == null || identity.Equals(GetDefault(identity.GetType()));
    }

    private static object? GetDefault(Type type)
    {
        return type.IsValueType ? Activator.CreateInstance(type) : null;
    }

    public override bool Equals(object? obj)
    {
        if (obj == null || obj.GetType() != GetType())
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        var other = (Entity)obj;

        if (IsTransient() || other.IsTransient())
        {
            return false;
        }

        return GetIdentity().Equals(other.GetIdentity());
    }

    public override int GetHashCode()
    {
        if (!IsTransient())
        {
            return GetIdentity().GetHashCode() ^ 31;
        }

        return base.GetHashCode();
    }

    public static bool operator ==(Entity? left, Entity? right)
    {
        if (left is null && right is null)
        {
            return true;
        }

        if (left is null || right is null)
        {
            return false;
        }

        return left.Equals(right);
    }

    public static bool operator !=(Entity? left, Entity? right)
    {
        return !(left == right);
    }
}