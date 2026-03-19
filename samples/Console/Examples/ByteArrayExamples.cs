using System;
using System.Collections.Generic;
using System.Linq;
using Simple.AutoMapper.Core;

namespace Simple.AutoMapper.Examples
{
    /// <summary>
    /// Demonstrates byte[] (primitive array) mapping — the PearlDental ProfilePhoto scenario.
    /// Before the fix, Mapper.Map would throw:
    ///   ArgumentException: Expression of type 'List`1[Byte]' cannot be used for assignment to type 'Byte[]'
    /// </summary>
    public static class ByteArrayExamples
    {
        public static void SingleEntityWithPhoto()
        {
            Console.WriteLine("── byte[] Map: Single Entity with ProfilePhoto ──");

            var photoBytes = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10 }; // JPEG header

            var entity = new MemberEntity
            {
                Id = "member-001",
                Email = "doctor@dental.com",
                FirstName = "John",
                LastName = "Doe",
                ProfilePhoto = photoBytes,
                ProfilePhotoContentType = "image/jpeg",
                EmailNotifications = true,
                SmsNotifications = false
            };

            var dto = Mapper.Map<MemberDto>(entity);

            Console.WriteLine($"  Entity → DTO: {dto.FirstName} {dto.LastName} ({dto.Email})");
            Console.WriteLine($"  ProfilePhoto: {(dto.ProfilePhoto != null ? $"{dto.ProfilePhoto.Length} bytes" : "null")}");
            Console.WriteLine($"  ContentType: {dto.ProfilePhotoContentType}");
            Console.WriteLine($"  Photo bytes match: {dto.ProfilePhoto.SequenceEqual(photoBytes)}");
            Console.WriteLine($"  Photo is clone (not same ref): {!ReferenceEquals(dto.ProfilePhoto, photoBytes)}");
            Console.WriteLine($"  EmailNotifications: {dto.EmailNotifications}");
            Console.WriteLine($"  SmsNotifications: {dto.SmsNotifications}");
            Console.WriteLine();
        }

        public static void SingleEntityWithNullPhoto()
        {
            Console.WriteLine("── byte[] Map: Single Entity with null ProfilePhoto ──");

            var entity = new MemberEntity
            {
                Id = "member-002",
                Email = "staff@dental.com",
                FirstName = "Jane",
                LastName = "Smith",
                ProfilePhoto = null,
                ProfilePhotoContentType = null
            };

            var dto = Mapper.Map<MemberDto>(entity);

            Console.WriteLine($"  Entity → DTO: {dto.FirstName} {dto.LastName}");
            Console.WriteLine($"  ProfilePhoto is null: {dto.ProfilePhoto == null}");
            Console.WriteLine($"  ContentType is null: {dto.ProfilePhotoContentType == null}");
            Console.WriteLine();
        }

        public static void CollectionWithPhotos()
        {
            Console.WriteLine("── byte[] Map: Collection (Admin member list scenario) ──");

            var entities = new List<MemberEntity>
            {
                new MemberEntity
                {
                    Id = "m1", Email = "a@test.com", FirstName = "Alice", LastName = "A",
                    ProfilePhoto = new byte[] { 0x89, 0x50, 0x4E, 0x47 }, // PNG header
                    ProfilePhotoContentType = "image/png"
                },
                new MemberEntity
                {
                    Id = "m2", Email = "b@test.com", FirstName = "Bob", LastName = "B",
                    ProfilePhoto = null,
                    ProfilePhotoContentType = null
                },
                new MemberEntity
                {
                    Id = "m3", Email = "c@test.com", FirstName = "Charlie", LastName = "C",
                    ProfilePhoto = new byte[] { 0xFF, 0xD8 }, // JPEG
                    ProfilePhotoContentType = "image/jpeg"
                }
            };

            // This was the exact call that crashed in PearlDental Admin:
            // Mapper.Map<MemberTbl, MemberDto>(entities)
            var dtos = Mapper.Map<MemberEntity, MemberDto>(entities);

            Console.WriteLine($"  Mapped {dtos.Count} members:");
            foreach (var dto in dtos)
            {
                var photoInfo = dto.ProfilePhoto != null ? $"{dto.ProfilePhoto.Length} bytes" : "null";
                Console.WriteLine($"    {dto.FirstName} {dto.LastName}: Photo={photoInfo}");
            }
            Console.WriteLine();
        }

        public static void PatchWithPhoto()
        {
            Console.WriteLine("── byte[] Patch: Update ProfilePhoto ──");

            var existing = new MemberDto
            {
                Id = "member-001",
                Email = "doctor@dental.com",
                FirstName = "John",
                LastName = "Doe",
                ProfilePhoto = new byte[] { 0x01, 0x02 },
                ProfilePhotoContentType = "image/png"
            };

            var update = new MemberEntity
            {
                Id = "member-001",
                Email = "doctor@dental.com",
                FirstName = "John",
                LastName = "Doe",
                ProfilePhoto = new byte[] { 0xFF, 0xD8, 0xFF },
                ProfilePhotoContentType = "image/jpeg"
            };

            Mapper.Patch(update, existing);

            Console.WriteLine($"  After patch: {existing.ProfilePhoto.Length} bytes, type={existing.ProfilePhotoContentType}");
            Console.WriteLine($"  Photo updated: {existing.ProfilePhoto.SequenceEqual(new byte[] { 0xFF, 0xD8, 0xFF })}");
            Console.WriteLine();
        }

        public static void RunAll()
        {
            Console.WriteLine("╔══════════════════════════════════════════════════╗");
            Console.WriteLine("║  byte[] (Primitive Array) Mapping Examples       ║");
            Console.WriteLine("║  Scenario: PearlDental Member.ProfilePhoto       ║");
            Console.WriteLine("╚══════════════════════════════════════════════════╝\n");

            SingleEntityWithPhoto();
            SingleEntityWithNullPhoto();
            CollectionWithPhotos();
            PatchWithPhoto();

            Console.WriteLine("All byte[] mapping examples passed!\n");
        }
    }
}
