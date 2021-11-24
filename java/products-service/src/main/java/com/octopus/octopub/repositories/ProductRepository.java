package com.octopus.octopub.repositories;

import com.github.tennaito.rsql.jpa.JpaPredicateVisitor;
import com.octopus.octopub.Constants;
import com.octopus.octopub.models.Product;
import cz.jirutka.rsql.parser.RSQLParser;
import cz.jirutka.rsql.parser.ast.Node;
import cz.jirutka.rsql.parser.ast.RSQLVisitor;
import java.util.List;
import java.util.stream.Collectors;
import javax.enterprise.context.ApplicationScoped;
import javax.inject.Inject;
import javax.persistence.EntityManager;
import javax.persistence.criteria.CriteriaBuilder;
import javax.persistence.criteria.CriteriaQuery;
import javax.persistence.criteria.From;
import javax.persistence.criteria.Predicate;
import lombok.NonNull;
import org.h2.util.StringUtils;

/**
 * Repositories are the interface between the application and the data store. They don't
 * contain any business logic, security rules, or manual audit logging. Note though that
 * we use Envers to automatically track database changes.
 */
@ApplicationScoped
public class ProductRepository {

  @Inject
  EntityManager em;

  public Product findOne(final int id) {
    return em.find(Product.class, id);
  }

  public void delete(final int id) {
    em.createQuery("delete from Product p where p.id=:id").setParameter("id", id).executeUpdate();
  }

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
      em.merge(existingProduct);
      return existingProduct;
    }

    return null;
  }

  public List<Product> findAll(@NonNull final List<String> partitions, final String filter) {

    final CriteriaBuilder builder = em.getCriteriaBuilder();
    final CriteriaQuery<Product> criteria = builder.createQuery(Product.class);
    final From<Product, Product> root = criteria.from(Product.class);

    // add the partition search rules
    final Predicate partitionPredicate =
        builder.or(
            partitions.stream().map(p -> builder.equal(root.get("dataPartition"), p)).collect(
                Collectors.toList()).toArray(new Predicate[0]));

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

    return em.createQuery(criteria).getResultList();
  }

  public Product save(@NonNull final Product product) {
    product.id = null;
    em.persist(product);
    em.flush();
    return product;
  }
}
