package javax.security.jacc;

/**
 * Static compilation of the Lambda results in the error "NoClassDefFoundError exception for
 * javax/security/jacc/PolicyContextException". Unfortunately, including the javax.security.jacc-api
 * dependency is not the correct solution: https://github.com/quarkusio/quarkus/pull/5343#issuecomment-739804358.
 *
 * Also including javax.security.jacc-api appears to pull in many dependencies on Java font and AWT
 * classes not supported by GraalVM.
 *
 * So we simply define this exception ourselves to allow static compilation.
 */
public class PolicyContextException extends Exception {
  public PolicyContextException() {
    super();
  }

  public PolicyContextException(String message) {
    super(message);
  }

  public PolicyContextException(String message, Throwable cause) {
    super(message, cause);
  }

  public PolicyContextException(Throwable cause) {
    super(cause);
  }
}
