using System.ComponentModel.DataAnnotations;

namespace WebApiAutores.Entidades
{
    public class Autor /*:IValidatableObject*/
    {
        public int Id { get; set; }

        [StringLength(maximumLength:120)]
        public string Name { get; set; }
        public List<AutorLibro> AutoresLibros { get; set; }


        //public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        //{
        //    if (!string.IsNullOrEmpty(Name))
        //    {
        //        var primeraletra = Name[0].ToString();
        //        if (primeraletra != primeraletra.ToUpper())
        //        {

        //            yield return new ValidationResult("La primera letra debe ser mayuscula",
        //                new string[] { nameof(Name) });
        //        }

        //    }

        //}
    }





}
