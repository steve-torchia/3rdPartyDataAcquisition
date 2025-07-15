namespace DP.Base
{
    public static class RegexPatterns
    {
        // https://stackoverflow.com/questions/2577236/regex-for-zip-code
        public const string USZipCode = @"^\d{5}(?:[-\s]\d{4})?$";

        //http://regexlib.com/Search.aspx?k=us+state&c=-1&m=-1&ps=20
        public const string USStates = @"^(?:(A[KLRZ]|C[AOT]|D[CE]|FL|GA|HI|I[ADLN]|K[SY]|LA|M[ADEINOST]|N[CDEHJMVY]|O[HKR]|P[AR]|RI|S[CD]|T[NX]|UT|V[AIT]|W[AIVY]))$";
    }
}