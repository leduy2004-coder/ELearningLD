using Application.Contracts;
using Application.DTOs.Request;
using Application.DTOs.Request.Email;
using Application.DTOs.Response;
using Application.Exceptions;
using AutoMapper;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Mail;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using WebAPI.Configurations;

namespace Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;
        private readonly EmailSettings _emailSettings;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _emailSettings = _configuration.GetSection("EmailSettings").Get<EmailSettings>();
        }

        public async Task SendEmailVerificationAsync(string email, string confirmationToken)
        {
            _logger.LogInformation("Sending email verification to {Email}", email);

            var verificationLink = $"{_emailSettings.ClientUrl}/verify-email?token={Uri.EscapeDataString(confirmationToken)}&email={Uri.EscapeDataString(email)}";

            var template = GetEmailVerificationTemplate(verificationLink);

            var emailRequest = new EmailRequest
            {
                To = email,
                Subject = template.Subject,
                Body = template.HtmlBody,
                IsHtml = true
            };

            await SendEmailAsync(emailRequest);

            _logger.LogInformation("Email verification sent successfully to {Email}", email);
        }

        public async Task SendPasswordResetEmailAsync(string email, string resetToken)
        {
            _logger.LogInformation("Sending password reset email to {Email}", email);

            var resetLink = $"{_emailSettings.ClientUrl}/reset-password?token={Uri.EscapeDataString(resetToken)}&email={Uri.EscapeDataString(email)}";

            var template = GetPasswordResetTemplate(resetLink);

            var emailRequest = new EmailRequest
            {
                To = email,
                Subject = template.Subject,
                Body = template.HtmlBody,
                IsHtml = true
            };

            await SendEmailAsync(emailRequest);

            _logger.LogInformation("Password reset email sent successfully to {Email}", email);
        }

        public async Task SendWelcomeEmailAsync(string email, string userName)
        {
            _logger.LogInformation("Sending welcome email to {Email}", email);

            var template = GetWelcomeTemplate(userName);

            var emailRequest = new EmailRequest
            {
                To = email,
                Subject = template.Subject,
                Body = template.HtmlBody,
                IsHtml = true
            };

            await SendEmailAsync(emailRequest);

            _logger.LogInformation("Welcome email sent successfully to {Email}", email);
        }

        public async Task SendPasswordChangedNotificationAsync(string email, string userName)
        {
            _logger.LogInformation("Sending password changed notification to {Email}", email);

            var template = GetPasswordChangedTemplate(userName);

            var emailRequest = new EmailRequest
            {
                To = email,
                Subject = template.Subject,
                Body = template.HtmlBody,
                IsHtml = true
            };

            await SendEmailAsync(emailRequest);

            _logger.LogInformation("Password changed notification sent successfully to {Email}", email);
        }

        public async Task SendOtpEmailAsync(string email, string otp, string purpose)
        {
            _logger.LogInformation("Sending OTP email to {Email} for purpose: {Purpose}", email, purpose);

            var template = GetOtpTemplate(otp, purpose);

            var emailRequest = new EmailRequest
            {
                To = email,
                Subject = template.Subject,
                Body = template.HtmlBody,
                IsHtml = true
            };

            await SendEmailAsync(emailRequest);

            _logger.LogInformation("OTP email sent successfully to {Email}", email);
        }

        private async Task SendEmailAsync(EmailRequest emailRequest)
        {
            try
            {
                using var smtpClient = new SmtpClient(_emailSettings.SmtpHost, _emailSettings.SmtpPort)
                {
                    Credentials = new NetworkCredential(_emailSettings.SmtpUser, _emailSettings.SmtpPassword),
                    EnableSsl = _emailSettings.EnableSsl,
                    DeliveryMethod = SmtpDeliveryMethod.Network
                };

                using var mailMessage = new MailMessage
                {
                    From = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName),
                    Subject = emailRequest.Subject,
                    Body = emailRequest.Body,
                    IsBodyHtml = emailRequest.IsHtml,
                    BodyEncoding = Encoding.UTF8,
                    SubjectEncoding = Encoding.UTF8
                };

                mailMessage.To.Add(emailRequest.To);

                // Add attachments if any
                foreach (var attachment in emailRequest.Attachments)
                {
                    if (File.Exists(attachment))
                    {
                        mailMessage.Attachments.Add(new Attachment(attachment));
                    }
                }

                await smtpClient.SendMailAsync(mailMessage);

                _logger.LogInformation("Email sent successfully to {Email}", emailRequest.To);
            }
            catch (SmtpException smtpEx)
            {
                _logger.LogError(smtpEx, "SMTP error occurred while sending email to {Email}: {Error}",
                    emailRequest.To, smtpEx.Message);
                throw new AppException(ErrorCode.EMAIL_SEND_FAILED);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while sending email to {Email}: {Error}",
                    emailRequest.To, ex.Message);
                throw new AppException(ErrorCode.EMAIL_SEND_FAILED);
            }
        }

        #region Email Templates

        private EmailTemplate GetEmailVerificationTemplate(string verificationLink)
        {
            return new EmailTemplate
            {
                Subject = "Verify Your Email Address - ELearningLD",
                HtmlBody = $@"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset='utf-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <title>Email Verification</title>
            </head>
            <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>
                <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 30px; text-align: center; border-radius: 10px 10px 0 0;'>
                    <h1 style='color: white; margin: 0; font-size: 28px;'>Welcome to ELearningLD!</h1>
                </div>
                
                <div style='background: #f8f9fa; padding: 30px; border-radius: 0 0 10px 10px; border: 1px solid #e9ecef;'>
                    <h2 style='color: #495057; margin-top: 0;'>Please verify your email address</h2>
                    
                    <p style='font-size: 16px; margin-bottom: 25px;'>
                        Thank you for joining ELearningLD! To complete your registration and start learning, 
                        please verify your email address by clicking the button below.
                    </p>
                    
                    <div style='text-align: center; margin: 30px 0;'>
                        <a href='{verificationLink}' 
                           style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); 
                                  color: white; 
                                  padding: 12px 30px; 
                                  text-decoration: none; 
                                  border-radius: 25px; 
                                  font-weight: bold; 
                                  display: inline-block;'>
                            Verify Email Address
                        </a>
                    </div>
                    
                    <p style='font-size: 14px; color: #6c757d; margin-top: 25px;'>
                        If the button doesn't work, copy and paste this link into your browser:<br>
                        <a href='{verificationLink}' style='color: #667eea; word-break: break-all;'>{verificationLink}</a>
                    </p>
                    
                    <p style='font-size: 14px; color: #6c757d; margin-top: 25px;'>
                        This verification link will expire in 24 hours for security reasons.
                    </p>
                    
                    <hr style='border: none; height: 1px; background: #e9ecef; margin: 25px 0;'>
                    
                    <p style='font-size: 12px; color: #6c757d; text-align: center; margin: 0;'>
                        If you didn't create an account with ELearningLD, please ignore this email.
                    </p>
                </div>
            </body>
            </html>",
                TextBody = $@"
            Welcome to ELearningLD!
            
            Please verify your email address by visiting this link:
            {verificationLink}
            
            This verification link will expire in 24 hours.
            
            If you didn't create an account with ELearningLD, please ignore this email."
            };
        }

        private EmailTemplate GetPasswordResetTemplate(string resetLink)
        {
            return new EmailTemplate
            {
                Subject = "Reset Your Password - ELearningLD",
                HtmlBody = $@"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset='utf-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <title>Password Reset</title>
            </head>
            <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>
                <div style='background: linear-gradient(135deg, #ff6b6b 0%, #ee5a24 100%); padding: 30px; text-align: center; border-radius: 10px 10px 0 0;'>
                    <h1 style='color: white; margin: 0; font-size: 28px;'>Password Reset Request</h1>
                </div>
                
                <div style='background: #f8f9fa; padding: 30px; border-radius: 0 0 10px 10px; border: 1px solid #e9ecef;'>
                    <h2 style='color: #495057; margin-top: 0;'>Reset your password</h2>
                    
                    <p style='font-size: 16px; margin-bottom: 25px;'>
                        We received a request to reset your password for your ELearningLD account. 
                        Click the button below to create a new password.
                    </p>
                    
                    <div style='text-align: center; margin: 30px 0;'>
                        <a href='{resetLink}' 
                           style='background: linear-gradient(135deg, #ff6b6b 0%, #ee5a24 100%); 
                                  color: white; 
                                  padding: 12px 30px; 
                                  text-decoration: none; 
                                  border-radius: 25px; 
                                  font-weight: bold; 
                                  display: inline-block;'>
                            Reset Password
                        </a>
                    </div>
                    
                    <p style='font-size: 14px; color: #6c757d; margin-top: 25px;'>
                        If the button doesn't work, copy and paste this link into your browser:<br>
                        <a href='{resetLink}' style='color: #ff6b6b; word-break: break-all;'>{resetLink}</a>
                    </p>
                    
                    <p style='font-size: 14px; color: #dc3545; margin-top: 25px; padding: 15px; background: #f8d7da; border-radius: 5px;'>
                        <strong>Security Notice:</strong> This password reset link will expire in 1 hour. 
                        If you didn't request this reset, please ignore this email and your password will remain unchanged.
                    </p>
                </div>
            </body>
            </html>"
            };
        }

        private EmailTemplate GetWelcomeTemplate(string userName)
        {
            return new EmailTemplate
            {
                Subject = "Welcome to ELearningLD - Start Your Learning Journey!",
                HtmlBody = $@"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset='utf-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <title>Welcome to ELearningLD</title>
            </head>
            <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>
                <div style='background: linear-gradient(135deg, #11998e 0%, #38ef7d 100%); padding: 30px; text-align: center; border-radius: 10px 10px 0 0;'>
                    <h1 style='color: white; margin: 0; font-size: 28px;'>Welcome to ELearningLD!</h1>
                </div>
                
                <div style='background: #f8f9fa; padding: 30px; border-radius: 0 0 10px 10px; border: 1px solid #e9ecef;'>
                    <h2 style='color: #495057; margin-top: 0;'>Hello {userName}! 🎓</h2>
                    
                    <p style='font-size: 16px; margin-bottom: 25px;'>
                        Thank you for joining ELearningLD! Your account has been successfully created and verified. 
                        You're now part of our learning community.
                    </p>
                    
                    <h3 style='color: #495057;'>What's Next?</h3>
                    <ul style='padding-left: 20px;'>
                        <li style='margin-bottom: 10px;'>Explore hundreds of courses across multiple fields</li>
                        <li style='margin-bottom: 10px;'>Track and save your learning progress</li>
                        <li style='margin-bottom: 10px;'>Follow your favorite instructors</li>
                        <li style='margin-bottom: 10px;'>Discover new knowledge tailored to you</li>
                        <li style='margin-bottom: 10px;'>Connect with other learners</li>
                    </ul>
                    
                    <div style='text-align: center; margin: 30px 0;'>
                        <a href='{_emailSettings.ClientUrl}' 
                           style='background: linear-gradient(135deg, #11998e 0%, #38ef7d 100%); 
                                  color: white; 
                                  padding: 12px 30px; 
                                  text-decoration: none; 
                                  border-radius: 25px; 
                                  font-weight: bold; 
                                  display: inline-block;'>
                            Start Learning Now
                        </a>
                    </div>
                    
                    <p style='font-size: 14px; color: #6c757d; text-align: center; margin-top: 25px;'>
                        Need help? Contact our support team at <a href='mailto:support@elearningld.com' style='color: #11998e;'>support@elearningld.com</a>
                    </p>
                </div>
            </body>
            </html>"
            };
        }

        private EmailTemplate GetPasswordChangedTemplate(string userName)
        {
            return new EmailTemplate
            {
                Subject = "Password Changed Successfully - ELearningLD",
                HtmlBody = $@"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset='utf-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <title>Password Changed</title>
            </head>
            <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>
                <div style='background: linear-gradient(135deg, #2c3e50 0%, #3498db 100%); padding: 30px; text-align: center; border-radius: 10px 10px 0 0;'>
                    <h1 style='color: white; margin: 0; font-size: 28px;'>Password Changed</h1>
                </div>
                
                <div style='background: #f8f9fa; padding: 30px; border-radius: 0 0 10px 10px; border: 1px solid #e9ecef;'>
                    <h2 style='color: #495057; margin-top: 0;'>Hello {userName},</h2>
                    
                    <p style='font-size: 16px; margin-bottom: 25px;'>
                        This is to confirm that your password has been successfully changed on 
                        <strong>{DateTime.UtcNow:MMMM dd, yyyy}</strong> at <strong>{DateTime.UtcNow:HH:mm} UTC</strong>.
                    </p>
                    
                    <div style='background: #d4edda; border: 1px solid #c3e6cb; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                        <p style='margin: 0; color: #155724;'>
                            <strong>✓ Security Update:</strong> Your account is now secured with your new password.
                        </p>
                    </div>
                    
                    <p style='font-size: 14px; color: #dc3545; margin-top: 25px; padding: 15px; background: #f8d7da; border-radius: 5px;'>
                        <strong>Didn't change your password?</strong> If you didn't make this change, 
                        please contact our support team immediately at 
                        <a href='mailto:security@elearningld.com' style='color: #dc3545;'>security@elearningld.com</a>
                    </p>
                    
                    <p style='font-size: 14px; color: #6c757d; text-align: center; margin-top: 25px;'>
                        For account security, you've been logged out of all devices. Please log in again with your new password.
                    </p>
                </div>
            </body>
            </html>"
            };
        }

        private EmailTemplate GetOtpTemplate(string otp, string purpose)
        {
            return new EmailTemplate
            {
                Subject = $"Your ELearningLD Verification Code: {otp}",
                HtmlBody = $@"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset='utf-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <title>Verification Code</title>
            </head>
            <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>
                <div style='background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%); padding: 30px; text-align: center; border-radius: 10px 10px 0 0;'>
                    <h1 style='color: white; margin: 0; font-size: 28px;'>Verification Code</h1>
                </div>
                
                <div style='background: #f8f9fa; padding: 30px; border-radius: 0 0 10px 10px; border: 1px solid #e9ecef;'>
                    <h2 style='color: #495057; margin-top: 0;'>Your verification code for {purpose}</h2>
                    
                    <p style='font-size: 16px; margin-bottom: 25px;'>
                        Use the following verification code to complete your request:
                    </p>
                    
                    <div style='text-align: center; margin: 30px 0;'>
                        <div style='background: #ffffff; 
                                   border: 2px dashed #f5576c; 
                                   padding: 20px; 
                                   border-radius: 10px; 
                                   display: inline-block;'>
                            <span style='font-size: 32px; 
                                        font-weight: bold; 
                                        color: #f5576c; 
                                        letter-spacing: 8px; 
                                        font-family: monospace;'>{otp}</span>
                        </div>
                    </div>
                    
                    <p style='font-size: 14px; color: #dc3545; margin-top: 25px; padding: 15px; background: #f8d7da; border-radius: 5px;'>
                        <strong>⏰ Important:</strong> This verification code will expire in 10 minutes for security reasons.
                    </p>
                    
                    <p style='font-size: 14px; color: #6c757d; margin-top: 25px;'>
                        If you didn't request this verification code, please ignore this email.
                    </p>
                </div>
            </body>
            </html>"
            };
        }
        #endregion
    }
}