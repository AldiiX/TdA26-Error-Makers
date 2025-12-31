using tda26.Server.Data.Models;

namespace tda26.Server.DTOs.Mapping;

public static class AuthorMapper {


	public static AuthorDto ToAuthorDto(this Account account) {
		if (account is Lecturer lecturer) {
			return new AuthorDto {
				Uuid = lecturer.Uuid,
				Username = lecturer.Username,
				PictureUrl = lecturer.PictureUrl,
				TitleBefore = lecturer.TitleBefore,
				FirstName = lecturer.FirstName,
				MiddleName = lecturer.MiddleName,
				LastName = lecturer.LastName,
				TitleAfter = lecturer.TitleAfter,
				Bio = lecturer.Bio
			};
		} else {
			return new AuthorDto {
				Uuid = account.Uuid,
				Username = account.Username,
				//PictureUrl = account.PictureUrl
			};
		}
	}
}