using System.Runtime.Serialization;


namespace Veneka.Indigo.Integration.Fidelity
{
    [DataContract]
    public class Customer 
    {
        [DataMember(Name= "number")]
        public string Number { get; set; }

        [DataMember(Name = "verificationStatus")]
        public string VerificationStatus { get; set; }

        [DataMember(Name = "employeeCode")]
        public string EmployeeCode { get; set; }

        [DataMember(Name = "requestCode")]
        public string RequestCode { get; set; }

        [DataMember(Name = "surname")]
        public string Surname { get; set; }

        [DataMember(Name = "otherNames")]
        public string OtherNames { get; set; }

        [DataMember(Name = "batchNumber")]
        public string BatchNumber { get; set; }

        [DataMember(Name = "requestTimeStamp")]
        public string RequestTimeStamp { get; set; }

        [DataMember(Name = "nationality")]
        public string Nationality { get; set; }

        [DataMember(Name = "nationalId")]
        public string NationalId { get; set; }

        [DataMember(Name = "dateOfBirth")]
        public string DateOfBirth { get; set; }

        [DataMember(Name = "internationalSerialNo")]
        public string InternationalSerialNo { get; set; }

        [DataMember(Name = "internationalCreatedDate")]
        public string InternationalCreatedDate { get; set; }

        [DataMember(Name = "internationalCardType")]
        public string InternationalCardType { get; set; }

        [DataMember(Name = "internationalExpiryDate")]
        public string InternationalExpiryDate { get; set; }

        [DataMember(Name = "localSerialNumber")]
        public string LocalSerialNumber { get; set; }

        [DataMember(Name = "localCreatedDate")]
        public string LocalCreatedDate { get; set; }

        [DataMember(Name = "localCardType")]
        public string LocalCardType { get; set; }

        [DataMember(Name = "localExpiryDate")]
        public string LocalExpiryDate { get; set; }

    }
}
