namespace SkyPlaneAR.API
{
    public readonly struct SkyPlaneARResult<T>
    {
        public readonly bool Success;
        public readonly T Value;
        public readonly string Error;

        private SkyPlaneARResult(bool success, T value, string error)
        {
            Success = success;
            Value = value;
            Error = error;
        }

        public static SkyPlaneARResult<T> Ok(T value) => new SkyPlaneARResult<T>(true, value, null);
        public static SkyPlaneARResult<T> Fail(string error) => new SkyPlaneARResult<T>(false, default, error);

        public override string ToString() =>
            Success ? $"Ok({Value})" : $"Fail({Error})";
    }
}
