﻿// ---------------------------------------------------------------
// Copyright (c) Coalition of the Good-Hearted Engineers
// FREE TO USE AS LONG AS SOFTWARE FUNDS ARE DONATED TO THE POOR
// ---------------------------------------------------------------

using System;
using System.Threading.Tasks;
using EFxceptions.Models.Exceptions;
using Moq;
using OtripleS.Web.Api.Models.Students;
using OtripleS.Web.Api.Models.Students.Exceptions;
using Xunit;

namespace OtripleS.Web.Api.Tests.Unit.Services.Foundations.Students
{
    public partial class StudentServiceTests
    {
        [Fact]
        public async void ShouldThrowValidationExceptionOnRegisterWhenStudentIsNullAndLogItAsync()
        {
            // given
            Student nullStudent = null;
            var nullStudentException = new NullStudentException();

            var expectedStudentValidationException =
                new StudentValidationException(nullStudentException);

            // when
            ValueTask<Student> registerStudentTask =
                this.studentService.RegisterStudentAsync(nullStudent);

            // then
            await Assert.ThrowsAsync<StudentValidationException>(() =>
                registerStudentTask.AsTask());

            this.loggingBrokerMock.Verify(broker =>
                broker.LogError(It.Is(SameExceptionAs(
                    expectedStudentValidationException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.InsertStudentAsync(It.IsAny<Student>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async void ShouldThrowValidationExceptionOnRegisterIfStudentIsInvalidAndLogItAsync(
            string invalidText)
        {
            // given
            var invalidStuent = new Student
            {
                UserId = invalidText,
                IdentityNumber = invalidText,
                FirstName = invalidText
            };

            var invalidStudentException = new InvalidStudentException();

            invalidStudentException.AddData(
                key: nameof(Student.Id),
                values: "Id cannot be empty.");

            invalidStudentException.AddData(
                key: nameof(Student.UserId),
                values: "Text cannot be null, empty or whitespace.");

            invalidStudentException.AddData(
                key: nameof(Student.IdentityNumber),
                values: "Text cannot be null, empty or whitespace.");

            invalidStudentException.AddData(
                key: nameof(Student.FirstName),
                values: "Text cannot be null, empty or whitespace.");

            invalidStudentException.AddData(
                key: nameof(Student.BirthDate),
                values: "Date cannot be default.");

            invalidStudentException.AddData(
                key: nameof(Student.CreatedBy),
                values: "Id cannot be empty.");

            invalidStudentException.AddData(
                key: nameof(Student.UpdatedBy),
                values: "Id cannot be empty.");

            invalidStudentException.AddData(
                key: nameof(Student.CreatedDate),
                values: "Date cannot be default.");

            invalidStudentException.AddData(
                key: nameof(Student.UpdatedDate),
                values: "Date cannot be default.");

            var expectedStudentValidationException =
                new StudentValidationException(invalidStudentException);

            // when
            ValueTask<Student> registerStudentTask =
                this.studentService.RegisterStudentAsync(invalidStuent);

            // then
            await Assert.ThrowsAsync<StudentValidationException>(() =>
                registerStudentTask.AsTask());

            this.loggingBrokerMock.Verify(broker =>
                broker.LogError(It.Is(SameValidationExceptionAs(
                    expectedStudentValidationException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.InsertStudentAsync(It.IsAny<Student>()),
                    Times.Never);

            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async void ShouldThrowValidationExceptionOnRegisterWhenUpdatedByIsNotSameToCreatedByAndLogItAsync()
        {
            // given
            DateTimeOffset dateTime = GetRandomDateTime();
            Student randomStudent = CreateRandomStudent(dateTime);
            Student invalidStudent = randomStudent;
            invalidStudent.UpdatedBy = Guid.NewGuid();

            var invalidStudentInputException = new InvalidStudentException();

            invalidStudentInputException.AddData(
                key: nameof(Student.UpdatedBy),
                values: $"Id is not the same as {nameof(Student.CreatedBy)}.");

            var expectedStudentValidationException =
                new StudentValidationException(invalidStudentInputException);

            // when
            ValueTask<Student> registerStudentTask =
                this.studentService.RegisterStudentAsync(invalidStudent);

            // then
            await Assert.ThrowsAsync<StudentValidationException>(() =>
                registerStudentTask.AsTask());

            this.loggingBrokerMock.Verify(broker =>
                broker.LogError(It.Is(SameValidationExceptionAs(
                    expectedStudentValidationException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.InsertStudentAsync(It.IsAny<Student>()),
                    Times.Never);

            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async void ShouldThrowValidationExceptionOnRegisterWhenUpdatedDateIsNotSameToCreatedDateAndLogItAsync()
        {
            // given
            DateTimeOffset dateTime = GetRandomDateTime();
            Student randomStudent = CreateRandomStudent(dateTime);
            Student inputStudent = randomStudent;
            inputStudent.UpdatedBy = randomStudent.CreatedBy;
            inputStudent.UpdatedDate = GetRandomDateTime();

            var invalidStudentInputException = new InvalidStudentException(
                parameterName: nameof(Student.UpdatedDate),
                parameterValue: inputStudent.UpdatedDate);

            var expectedStudentValidationException =
                new StudentValidationException(invalidStudentInputException);

            // when
            ValueTask<Student> registerStudentTask =
                this.studentService.RegisterStudentAsync(inputStudent);

            // then
            await Assert.ThrowsAsync<StudentValidationException>(() =>
                registerStudentTask.AsTask());

            this.loggingBrokerMock.Verify(broker =>
                broker.LogError(It.Is(SameExceptionAs(expectedStudentValidationException))),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.InsertStudentAsync(It.IsAny<Student>()),
                    Times.Never);

            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }

        [Theory]
        [MemberData(nameof(InvalidMinuteCases))]
        public async void ShouldThrowValidationExceptionOnRegisterWhenCreatedDateIsNotRecentAndLogItAsync(
            int minutes)
        {
            // given
            DateTimeOffset dateTime = GetRandomDateTime();
            Student randomStudent = CreateRandomStudent(dateTime);
            Student inputStudent = randomStudent;
            inputStudent.UpdatedBy = inputStudent.CreatedBy;
            inputStudent.CreatedDate = dateTime.AddMinutes(minutes);
            inputStudent.UpdatedDate = inputStudent.CreatedDate;

            var invalidStudentInputException = new InvalidStudentException(
                parameterName: nameof(Student.CreatedDate),
                parameterValue: inputStudent.CreatedDate);

            var expectedStudentValidationException =
                new StudentValidationException(invalidStudentInputException);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTime())
                    .Returns(dateTime);

            // when
            ValueTask<Student> registerStudentTask =
                this.studentService.RegisterStudentAsync(inputStudent);

            // then
            await Assert.ThrowsAsync<StudentValidationException>(() =>
                registerStudentTask.AsTask());

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTime(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogError(It.Is(SameExceptionAs(expectedStudentValidationException))),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.InsertStudentAsync(It.IsAny<Student>()),
                    Times.Never);

            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async void ShouldThrowValidationExceptionOnRegisterWhenStudentAlreadyExistsAndLogItAsync()
        {
            // given
            DateTimeOffset dateTime = GetRandomDateTime();
            Student randomStudent = CreateRandomStudent(dateTime);
            Student alreadyExistsStudent = randomStudent;
            alreadyExistsStudent.UpdatedBy = alreadyExistsStudent.CreatedBy;
            string randomMessage = GetRandomMessage();
            string exceptionMessage = randomMessage;
            var duplicateKeyException = new DuplicateKeyException(exceptionMessage);

            var alreadyExistsStudentException =
                new AlreadyExistsStudentException(duplicateKeyException);

            var expectedStudentValidationException =
                new StudentValidationException(alreadyExistsStudentException);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTime())
                    .Returns(dateTime);

            this.storageBrokerMock.Setup(broker =>
                broker.InsertStudentAsync(alreadyExistsStudent))
                    .ThrowsAsync(duplicateKeyException);

            // when
            ValueTask<Student> registerStudentTask =
                this.studentService.RegisterStudentAsync(alreadyExistsStudent);

            // then
            await Assert.ThrowsAsync<StudentValidationException>(() =>
                registerStudentTask.AsTask());

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTime(),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.InsertStudentAsync(alreadyExistsStudent),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
               broker.LogError(It.Is(SameExceptionAs(expectedStudentValidationException))),
                    Times.Once);

            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
