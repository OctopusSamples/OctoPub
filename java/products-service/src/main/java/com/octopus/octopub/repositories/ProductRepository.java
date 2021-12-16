package com.octopus.octopub.repositories;

import com.github.tennaito.rsql.jpa.JpaPredicateVisitor;
import com.octopus.octopub.exceptions.InvalidInput;
import com.octopus.octopub.models.Product;
import cz.jirutka.rsql.parser.RSQLParser;
import cz.jirutka.rsql.parser.ast.Node;
import cz.jirutka.rsql.parser.ast.RSQLVisitor;
import java.util.List;
import java.util.Set;
import java.util.stream.Collectors;
import javax.enterprise.context.ApplicationScoped;
import javax.inject.Inject;
import javax.persistence.EntityManager;
import javax.persistence.criteria.CriteriaBuilder;
import javax.persistence.criteria.CriteriaQuery;
import javax.persistence.criteria.From;
import javax.persistence.criteria.Predicate;
import javax.validation.ConstraintViolation;
import javax.validation.Validator;
import lombok.NonNull;
import org.h2.util.StringUtils;

/**
 * Repositories are the interface between the application and the data store. They don't contain any
 * business logic, security rules, or manual audit logging. Note though that we use Envers to
 * automatically track database changes.
 */
@ApplicationScoped
public class ProductRepository {

  @Inject EntityManager em;

  @Inject Validator validator;

  /**
   * Get a single entity.
   *
   * @param id The ID of the entity to update.
   * @return The entity.
   */
  public Product findOne(final int id) {
    final Product product = em.find(Product.class, id);
    if (product != null) {
      em.detach(product);
    }
    return product;
  }

  public void delete(final int id) {
    em.createQuery("delete from Product p where p.id=:id").setParameter("id", id).executeUpdate();
  }

  /**
   * Updates an existing entity in the data store.
   *
   * @param product The fields to update.
   * @param id The ID of the entity to update.
   * @return The updated entity.
   */
  public Product update(@NonNull final Product product, @NonNull final Integer id) {
    final Product existingProduct = em.find(Product.class, id);
    if (existingProduct != null) {
      if (product.name != null) {
        existingProduct.name = product.name;
      }
      if (product.description != null) {
        existingProduct.description = product.description;
      }
      if (product.epub != null) {
        existingProduct.epub = product.epub;
      }
      if (product.image != null) {
        existingProduct.image = product.image;
      }
      if (product.pdf != null) {
        existingProduct.pdf = product.pdf;
      }

      validateProduct(existingProduct);

      return existingProduct;
    }

    return null;
  }

  /**
   * Returns all matching entities.
   *
   * @param partitions The partitions that entities can be found in.
   * @param filter The RSQL filter used to query the entities.
   * @return The matching entities.
   */
  public List<Product> findAll(@NonNull final List<String> partitions, final String filter) {

    final CriteriaBuilder builder = em.getCriteriaBuilder();
    final CriteriaQuery<Product> criteria = builder.createQuery(Product.class);
    final From<Product, Product> root = criteria.from(Product.class);

    // add the partition search rules
    final Predicate partitionPredicate =
        builder.or(
            partitions.stream()
                .map(p -> builder.equal(root.get("dataPartition"), p))
                .collect(Collectors.toList())
                .toArray(new Predicate[0]));

    if (!StringUtils.isNullOrEmpty(filter)) {
      /*
       Makes use of RSQL queries to filter any responses:
       https://github.com/jirutka/rsql-parser
      */
      final RSQLVisitor<Predicate, EntityManager> visitor =
          new JpaPredicateVisitor<Product>().defineRoot(root);
      final Node rootNode = new RSQLParser().parse(filter);
      final Predicate filterPredicate = rootNode.accept(visitor, em);

      // combine with the filter rules
      final Predicate combinedPredicate = builder.and(partitionPredicate, filterPredicate);

      criteria.where(combinedPredicate);
    } else {
      criteria.where(partitionPredicate);
    }

    final List<Product> results = em.createQuery(criteria).getResultList();

    // detach all the entities
    em.clear();

    return results;
  }

  /**
   * Saves a new product in the data store.
   *
   * @param product The product to save.
   * @return The newly created entity.
   */
  public Product save(@NonNull final Product product) {
    product.id = null;

    validateProduct(product);

    em.persist(product);
    em.flush();
    return product;
  }

  private void validateProduct(@NonNull final Product product) {
    final Set<ConstraintViolation<Product>> violations = validator.validate(product);
    if (violations.isEmpty()) {
      return;
    }

    throw new InvalidInput(
        violations.stream().map(cv -> cv.getMessage()).collect(Collectors.joining(", ")));
  }
}
